"""Full-universe source ownership, preserving historical704 and every C8 proof."""
from common_v2 import *
from proof_kernel_v1 import *
from collections import Counter
u=json.loads((HERE/'current-universe-packet-v1.json').read_bytes());prior=json.loads((PREVIOUS/'current-ownership-v2.json').read_bytes());oldh=json.loads((PREVIOUS/'historical-ownership-v2.json').read_bytes())['members'];original=json.loads((W2.parent/'w1/original-universe.json').read_bytes());contracts=json.loads((HERE/'whole-body-contracts-v1.json').read_bytes())['contracts'];callbacks=json.loads((HERE/'callback-proofs-v1.json').read_bytes())['rows'];mapping=map_current(prior['members'],u['members']);reverse={n['symbol_id']:p for p,n in mapping.items()};oldm={n['symbol_id']:n for n in prior['members']}
inherited={mapping[n['symbol_id']]['symbol_id'] for n in prior['members'] if not n['review_status'].startswith('OPEN')}
for n in prior['members']:
    if n['symbol_id'] in {r['symbol_id'] for r in prior['members'] if not r['review_status'].startswith('OPEN')}:need(mapping[n['symbol_id']]['body_sha256']==n['body_sha256'],'Inherited complete body bytes changed')
reviewed={c['current_id'] for c in contracts+callbacks};closed=inherited|reviewed;new_hist={c['original_id']:c for c in contracts+callbacks if c['new_support'] and c['original_id']}
history=[];new=[]
for old in oldh:
    row=dict(old);row['current_ids']=[mapping[k]['symbol_id'] for k in old['current_ids']]
    if old['canonical_original_id'] in new_hist:
        need(not old['proof'],'Reopened/recountered historical proof');c=new_hist[old['canonical_original_id']];row['proof']=dict(kind='C9_COMPLETE_SOURCE_CIL_BRANCH_INLINE_CALLBACK_CONTEXT',evidence='historical-callback-contracts-v1.json' if c in callbacks else 'whole-body-contracts-v1.json',original_id=old['canonical_original_id'],current_id=c['current_id']);row['disposition']='FULLY_SUPPORTED_C9_SOURCE_AND_COMPILED_CONTEXT'
        new.append(dict(original_id=old['canonical_original_id'],current_ids=row['current_ids'],source_contract=c,proof=row['proof']))
    history.append(row)
need(len(history)==len(oldh)==704,'Historical704 identity population changed')
current=[]
for n in u['members']:
    row=dict(n);old=oldm[reverse[n['symbol_id']]];row['review_status']='REVIEWED_COMPLETE_BODY_OR_EXPLICIT_CALLBACK' if n['symbol_id'] in closed else old['review_status'];current.append(row)
old_support={(s['path'],s['kind'],s['owner'],s['declaration']):s for s in prior['support']};meta=json.loads((HERE/'metadata-all/packet-v1-context-v5/current-Debug.json').read_bytes());types={t['type']:t for t in meta['types'] if t['type'] is not None};support=[];new_support=[]
for s in u['supports']:
    old=old_support[s['path'],s['kind'],s['owner'],s['declaration']];row=dict(s);supported=old['fully_reconciled'];proof=old.get('proof');children=[]
    if s['kind']=='ModuleBlock':
        match=__import__('re').search(r'\bModule\s+(\w+)',s['declaration']);owner=s['owner']+'.'+match[1];children=[n['symbol_id'] for n in u['members'] if n['owner']==owner]
        if owner!='BESHStatNG.UdfDataImport' and children and all(c in closed for c in children) and not supported:
            need(owner in ['BESHStatNG.WorksheetFunctions.AgreementMethodUDFs','BESHStatNG.WorksheetFunctions.ClassificationReportUDFs','BESHStatNG.UDFhelpers'],'Unreviewed new container candidate '+owner)
            context=types[owner];need(not context['fields'],'Unresolved container field/initializer');supported=True;proof=dict(kind='C9_ALL_CHILDREN_FULLY_SUPPORTED_NO_FIELDS_OR_INITIALIZER',children=children,context=context,evidence='whole-body-contracts-v1.json');new_support.append(s['support_id'])
    row.update(fully_reconciled=supported,proof=proof,children=children,distinct=not s['already_in_primary_population']);support.append(row)
closed_support={s['support_id'] for s in support if s['fully_reconciled']}
def headers(universe,ids):
    out={}
    for s in universe['supports']:
        if s['support_id'] not in ids or s['kind']!='ModuleBlock':continue
        for f in universe['fragments']:
            if f['member_id'] is None and f['path']==s['path'] and f['kind'] in ['ModuleStatement','EndModuleStatement'] and s['span_start']<=f['span_start'] and f['span_start']+f['span_length']<=s['span_start']+s['span_length']:out[f['fragment_id']]=s['support_id']
    return out
hc=headers(u,closed_support);closed_support_keys={(s['path'],s['kind'],s['owner'],s['declaration']) for s in support if s['fully_reconciled']};ho=headers(original,{s['support_id'] for s in original['supports'] if (s['path'],s['kind'],s['owner'],s['declaration']) in closed_support_keys});original_closed={h['git_original_id'] for h in history if h['proof']}
fragments=[dict(f,review_status='COVERED_BY_REVIEWED_BODY_OR_DECLARATION' if f['member_id'] in closed or f['fragment_id'] in hc else 'OPEN_SOURCE_FRAGMENT_RECONCILIATION_REQUIRED') for f in u['fragments']]
origrows=[dict(fragment_id=f['fragment_id'],parent=f['member_id'],path=f['path'],kind=f['kind'],is_branch=f['is_branch'],is_inline=f['is_inline'],status='COVERED_BY_PROVEN_BODY_OR_DECLARATION' if f['member_id'] in original_closed or f['fragment_id'] in ho else 'OPEN_SOURCE_FRAGMENT_RECONCILIATION_REQUIRED') for f in original['fragments']]
def counts(fs,covered):return dict(total=len(fs),branches=sum(f['is_branch'] for f in fs),inline=sum(f['is_inline'] for f in fs),covered=sum(covered(f) for f in fs),branches_covered=sum(f['is_branch'] and covered(f) for f in fs),inline_covered=sum(f['is_inline'] and covered(f) for f in fs))
summary=dict(status='NOT_READY_04A38',W2='IN_PROGRESS',W3='NOT_STARTED',historical=dict(total=704,closed=sum(bool(h['proof']) for h in history),OPEN=sum(not h['proof'] for h in history)),current=dict(total=len(current),closed=len(closed),OPEN=len(current)-len(closed)),support=dict(raw=len(support),distinct=sum(s['distinct'] for s in support),distinct_closed=sum(s['distinct'] and s['fully_reconciled'] for s in support),distinct_OPEN=sum(s['distinct'] and not s['fully_reconciled'] for s in support)),fragments=dict(current=counts(u['fragments'],lambda f:f['member_id'] in closed or f['fragment_id'] in hc),original=counts(original['fragments'],lambda f:f['member_id'] in original_closed or f['fragment_id'] in ho)),original_queries=0)
save('historical-ownership-v1.json',dict(members=history,original_queries=0));save('current-ownership-v1.json',dict(members=current,support=support,fragments=fragments));save('ownership-summary-v1.json',summary);save('original-fragment-parent-crosswalk-v1.json',dict(rows=origrows));save('new-closures-v1.json',dict(rows=new,new_historical=len(new),new_current_supported=len(closed-inherited),new_current_declarations=0,new_current_callbacks=sum(c['new_support'] for c in callbacks),new_support=new_support,reopened=[],original_queries=0))
queue=json.loads((PREVIOUS/'remaining-work-queue-v1.json').read_bytes());remaining=[dict(r,current_ids=[mapping[k]['symbol_id'] for k in r['current_ids']]) if 'current_ids' in r else dict(r) for r in queue['members'] if r['original_id'] not in new_hist];current_open=[dict(current_id=n['symbol_id'],name=n['name'],path=n['path'],reason='Whole-body/source/dependency/branch/inline obligation remains unresolved') for n in u['members'] if n['symbol_id'] not in closed]
save('remaining-work-queue-v1.json',dict(status='NOT_READY_04A38',members=remaining,first_members=remaining[:24],current_OPEN=current_open,ordering='Exact C8 machine order filtered only by genuinely supported original IDs; all other OPEN obligations retained',original_queries=0));save('current-semantic-position-crosswalk-v1.json',dict(rows=[dict(before=k,after=n['symbol_id'],body_bytes_equal=oldm[k]['body_sha256']==n['body_sha256']) for k,n in mapping.items()],old=726,current=len(current),shifted=sum(k!=n['symbol_id'] for k,n in mapping.items()),changed_existing=sum(oldm[k]['body_sha256']!=n['body_sha256'] for k,n in mapping.items()),new_declarations=[]))
print(json.dumps(summary));print('New historical/current/support',len(new),len(closed-inherited),len(new_support))
