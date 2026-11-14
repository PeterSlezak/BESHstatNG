"""Exact NEW/RETAINED/OPEN transitions and exhaustive source/fragment complements."""
from common import *
from proof_kernel_v8 import map_current
from source_views import view
import csv
from collections import Counter

load=lambda path:json.loads(path.read_bytes())
old=load(PREVIOUS/'current-ownership-v2.json');oh=load(PREVIOUS/'historical-ownership-v2.json')['members']
u=load(HERE/'final-v1/current-universe.json');o=load(W2.parent/'w1/original-universe.json')
cs=load(HERE/'whole-body-contracts-v1.json')['contracts'];cb=load(HERE/'callback-proofs-v1.json')['rows'];props=load(HERE/'property-body-contracts-v1.json')['contracts']
assert load(HERE/'managed-validation-v2.json')['Debug']['outcomes']==dict(Passed=20990,Failed=14)
assert len(load(HERE/'managed-Debug-identities-v2.json')['new_tests'])==64
assert load(HERE/'source-composition-proof-v2.json')['status']=='COMPLETE_ORIGINAL_ENTRY_AND_CORRECTED_CURRENT_SOURCE_COMPOSITES_PROVED'
mapping=map_current(old['members'],u['members']);reverse={n['symbol_id']:key for key,n in mapping.items()};oldm={n['symbol_id']:n for n in old['members']}
inherited={mapping[n['symbol_id']]['symbol_id'] for n in old['members'] if not n['review_status'].startswith('OPEN')}
assert len(inherited)==529 and all(mapping[n['symbol_id']]['body_sha256']==n['body_sha256'] for n in old['members'] if not n['review_status'].startswith('OPEN'))
eligible={r['current_id'] for r in cs+cb};closed=inherited|eligible
new={r['original_id']:r for r in cs+cb+props if r['new_support']}
assert len(new)==41 and len(closed-inherited)==37 and None not in new
history=[];newrows=[]
for h in oh:
    row=dict(h,current_ids=[mapping[key]['symbol_id'] for key in h['current_ids']])
    if h['canonical_original_id'] in new:
        r=new[h['canonical_original_id']];assert not h['proof']
        expected=[] if r in props else [r['current_id']]
        assert row['current_ids']==expected
        evidence='property-body-contracts-v1.json' if r in props else 'callback-proofs-v1.json' if r in cb else 'whole-body-contracts-v1.json'
        row.update(proof=dict(kind='C15_COMPLETE_ORIGINAL_CURRENT_BODY_SIGNATURE_CIL_CALLBACK_SOURCE_COMPOSITE_AND_TYPED_HOST_BOUNDARY',
            evidence=evidence,original_id=h['canonical_original_id'],current_ids=expected,
            behavior='source-composition-proof-v2.json',compiled_owner='host-cache-contexts-v1.json',
            complete_dependencies=['complete-compiled-dependency-catalog-v1.json','complete-original-compiled-dependency-catalog-v1.json']),
            disposition='FULLY_SUPPORTED_C15_COMPLETE_RECONCILED_BODY_CONTEXT')
        newrows.append(dict(original_id=h['canonical_original_id'],current_ids=expected,name=h['name'],proof=row['proof']))
    history.append(row)
members=[dict(n,review_status='FULLY_SUPPORTED_C15_COMPLETE_SOURCE_CIL_AND_OWNERSHIP_CONTEXT' if n['symbol_id'] in closed else oldm[reverse[n['symbol_id']]]['review_status']) for n in u['members']]
key=lambda s:(s['path'],s['kind'],s['owner'],s['declaration'])
oldss={key(s):s for s in old['support']}
contexts=load(HERE/'host-cache-contexts-v1.json')['rows']
data_classes={r['source_support']['support_id']:r for r in contexts}
module_names={'Public Module CoxUDFs','Friend Module UdfRegressionOutput'}


def children(s):
    base=s['owner'].rstrip('.')+'.'+s['declaration'].split()[-1]
    return [n for n in u['members'] if n['path']==s['path'] and (n['owner']==base or n['owner'].startswith(base+'.'))]


def complete(s):
    if oldss[key(s)]['fully_reconciled']:return True
    if s['support_id'] in data_classes:
        return all(n['symbol_id'] in closed for n in children(s))
    if s['kind']=='ModuleBlock' and s['declaration'] in module_names:
        nested=[v for v in u['supports'] if v['path']==s['path'] and v['support_id']!=s['support_id'] and s['span_start']<=v['span_start'] and v['span_start']+v['span_length']<=s['span_start']+s['span_length']]
        return all(n['symbol_id'] in closed for n in children(s)) and all(complete(v) for v in nested)
    return False


snapshots={r['source']['path']:r['snapshot'] for r in load(ROOT/'evidence/04A38/preedit/entry-source-snapshot.json')['files']}
supports=[];support_proofs=[]
for s in u['supports']:
    was=oldss[key(s)];done=complete(s);proof=was.get('proof')
    if done and not was['fully_reconciled']:
        proof=dict(kind='C15_COMPLETE_CONTAINER_SOURCE_CTOR_ACCESSOR_AND_ALL_CHILD_OWNERSHIP',evidence='support-container-proofs-v1.json',support_id=s['support_id'])
        archived=next(v for v in o['supports'] if key(v)==key(s))
        text,nv=view((ROOT/s['path']).read_bytes(),next(v['sha256'] for v in u['sources'] if v['path']==s['path']))
        before,bv=view((ROOT/snapshots[s['path']]['path']).read_bytes(),next(v['sha256'] for v in o['sources'] if v['path']==s['path']))
        part=lambda text,n:text.encode('utf-16-le')[n['span_start']*2:(n['span_start']+n['span_length'])*2].decode('utf-16-le')
        support_proofs.append(dict(current_source=s,original_source=archived,current_whole_declaration=part(text,s),original_whole_declaration=part(before,archived),
            current_view=nv,original_view=bv,original_snapshot=snapshots[s['path']],all_primary_children=children(s),
            blocking_primary_ids=[n['symbol_id'] for n in children(s) if n['symbol_id'] not in closed],
            state_and_compiled_evidence='host-cache-contexts-v1.json' if s['kind']=='ClassBlock' else 'constructed-owner-proofs-v1.json'))
    supports.append(dict(s,fully_reconciled=done,proof=proof,children=was.get('children',[]),distinct=not s['already_in_primary_population']))
new_support=[s['support_id'] for s in supports if s['fully_reconciled'] and not oldss[key(s)]['fully_reconciled']]
assert len(new_support)==7
assert {s['declaration'] for s in supports if s['support_id'] in new_support}==module_names|{'Private Class CoxModelHandle','Private Class GeeHandle','Private Class GeeLsmEstimateProfileValue','Private Class GeeLsmEstimateComponent','Private Class GeeLsmEstimateAtProfile'}
old_headers=load(PREVIOUS/'support-fragment-crosswalk-v1.json')
old_header_classes={key(s) for s in old['support'] if s['kind']=='ClassBlock' and s['support_id'] in set(old_headers['current'].values())}
old_class_owners={owner.rstrip('.')+'.'+declaration.split()[-1] for path,kind,owner,declaration in old_header_classes}
new_keys={key(s) for s in supports if s['support_id'] in new_support}


def headers(universe):
    result={};supported={key(s) for s in supports if s['fully_reconciled']}
    for s in universe['supports']:
        if key(s) not in supported:continue
        for f in universe['fragments']:
            if f['member_id'] is not None or f['path']!=s['path'] or not (s['span_start']<=f['span_start'] and f['span_start']+f['span_length']<=s['span_start']+s['span_length']):continue
            if key(s) in new_keys:
                result[f['fragment_id']]=s['support_id']
            elif s['kind']=='ModuleBlock' and f['kind'] in ['ModuleStatement','EndModuleStatement']:
                result[f['fragment_id']]=s['support_id']
            elif s['kind']=='ClassBlock' and key(s) in old_header_classes and f['kind'] in ['ClassStatement','EndClassStatement']:
                result[f['fragment_id']]=s['support_id']
            elif s['kind']=='PropertyStatement' and s['owner'] in old_class_owners and f['kind']=='PropertyStatement':
                result[f['fragment_id']]=s['support_id']
    return result


hc=headers(u);ho=headers(o);historical_closed={h['git_original_id'] for h in history if h['proof']}
fragments=[dict(f,review_status='FULLY_SUPPORTED_COMPLETE_SOURCE_COMPILED_CONTEXT' if f['member_id'] in closed or f['fragment_id'] in hc else 'OPEN',proof_member=f['member_id'] if f['member_id'] in closed else hc.get(f['fragment_id'])) for f in u['fragments']]
original_fragments=[dict(f,covered=f['member_id'] in historical_closed or f['fragment_id'] in ho,proof_member=f['member_id'] if f['member_id'] in historical_closed else ho.get(f['fragment_id'])) for f in o['fragments']]
count=lambda rows,p:dict(total=len(rows),covered=sum(p(r) for r in rows),branches=sum(r['is_branch'] for r in rows),branches_covered=sum(r['is_branch'] and p(r) for r in rows),inline=sum(r['is_inline'] for r in rows),inline_covered=sum(r['is_inline'] and p(r) for r in rows))
summary=dict(historical=dict(total=704,closed=537,OPEN=167),current=dict(total=726,closed=len(closed),OPEN=726-len(closed)),
    support=dict(raw=len(supports),distinct=303,distinct_closed=sum(s['fully_reconciled'] for s in supports if s['distinct']),distinct_OPEN=sum(not s['fully_reconciled'] for s in supports if s['distinct'])),
    fragments=dict(current=count(fragments,lambda f:f['review_status']!='OPEN'),original=count(original_fragments,lambda f:f['covered'])))
assert sum(bool(h['proof']) for h in history)==537 and (len(closed),summary['support']['distinct_closed'])==(566,263)
save('historical-ownership-v2.json',dict(members=history,total=704));save('current-ownership-v2.json',dict(members=members,support=supports,fragments=fragments))
save('ownership-summary-v2.json',summary);save('support-container-proofs-v1.json',dict(rows=support_proofs,original_queries=0))
save('support-fragment-crosswalk-v1.json',dict(current=hc,original=ho,qualification='Retained header policy is unchanged. Newly proved complete data/module containers additionally cover their real memberless declaration/initializer fragments using full constructor/field/source proofs.'))
save('original-fragment-parent-crosswalk-v2.json',dict(rows=original_fragments,original_queries=0))
save('new-closures-final-v1.json',dict(rows=newrows,new_support=new_support,historical_new=41,current_new=37,distinct_support_new=7,current_only=[],
    moved_property_historical_only=4,prior_C14_35_historical_35_current_3_support='ALL_RETAINED',prior_four_ROC_bodies='ALL_RETAINED_NO_REOPEN',original_queries=0))
priorq=load(PREVIOUS/'remaining-work-queue-v1.json');remaining=[r for r in priorq['members'] if r['original_id'] not in new]
save('remaining-work-queue-v1.json',dict(members=remaining,first_members=remaining[:24],
    current_OPEN=[dict(current_id=n['symbol_id'],name=n['name'],path=n['path'],next='Complete entire original/current body, transitive ownership, source/CIL/fragment/native boundary proof; no new original reference authority.') for n in members if n['review_status'].startswith('OPEN')],
    support_OPEN=[s for s in supports if s['distinct'] and not s['fully_reconciled']],original_queries=0))
assert len(remaining)==167
ledger=[]
for h,prior_h in zip(history,oh):
    ledger.append(dict(population='historical',identity=h['canonical_original_id'],transition='NEW' if h['canonical_original_id'] in new else 'RETAINED' if h['proof'] else 'OPEN',prior_supported=bool(prior_h['proof']),supported=bool(h['proof']),current_ids=h['current_ids'],proof=h['proof']))
for n in members:
    before=oldm[reverse[n['symbol_id']]];was=not before['review_status'].startswith('OPEN');done=n['symbol_id'] in closed
    ledger.append(dict(population='current',identity=n['symbol_id'],before_identity=before['symbol_id'],transition='RETAINED' if was else 'NEW' if done else 'OPEN',prior_supported=was,supported=done))
for s in supports:
    if not s['distinct']:continue
    before=oldss[key(s)];was=before['fully_reconciled'];done=s['fully_reconciled']
    ledger.append(dict(population='support',identity=s['support_id'],before_identity=before['support_id'],declaration=s['declaration'],owner=s['owner'],transition='RETAINED' if was else 'NEW' if done else 'OPEN',prior_supported=was,supported=done,proof=s['proof']))
assert len(ledger)==1733
save('canonical-identity-status-v1.json',dict(entries=ledger,scope='Exact704 historical /726 current /303 distinct support identities; no source IDs invented. Four original GEE properties have genuine inherited Common counterparts and no fabricated current Windows member IDs.'))
with (HERE/'canonical-identity-status-v1.csv').open('x',encoding='utf-8',newline='') as stream:
    writer=csv.DictWriter(stream,fieldnames=['population','identity','before_identity','transition','prior_supported','supported']);writer.writeheader()
    writer.writerows({k:r.get(k,'') for k in writer.fieldnames} for r in ledger)
save('readiness-v1.json',dict(status='NOT_READY_04A38',W2='IN_PROGRESS',W3='NOT_STARTED',P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED',science='NOT_GOLDEN',phase04A39='BLOCKED',
    mandatory_blockers=dict(historical_OPEN=167,current_OPEN=160,distinct_support_OPEN=40),owner_acceptance='NOT_ACCEPTED_NOT_MERGED',original_queries=0))
print(json.dumps(summary))
