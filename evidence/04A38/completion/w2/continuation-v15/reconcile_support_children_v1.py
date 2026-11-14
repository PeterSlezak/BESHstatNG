"""Correct current support child IDs from the actual census; retain all old bytes."""
from common import *
load=lambda p:json.loads(p.read_bytes())
u=load(HERE/'final-v1/current-universe.json');cur=load(HERE/'current-ownership-v2.json')
actual={s['support_id']:s for s in u['supports']};corrections=[];rows=[]
for s in cur['support']:
 a=actual[s['support_id']];need_children=a.get('children',[])
 if s['children']!=need_children:corrections.append(dict(support_id=s['support_id'],before_children=s['children'],actual_current_children=need_children,reason='The draft copied prior support children while updating the surrounding current source/span. Actual census child identities must move with source spans; prior identity bytes remain immutable in v2.'))
 rows.append(dict(s,children=need_children))
assert corrections and len(rows)==325
save('current-ownership-v3.json',dict(cur,support=rows))
queue=load(HERE/'remaining-work-queue-v1.json');byid={r['support_id']:r for r in rows}
save('remaining-work-queue-v2.json',dict(queue,support_OPEN=[byid[r['support_id']] for r in queue['support_OPEN']]))
save('support-child-identity-reconciliation-v1.json',dict(status='EXACT_ACTUAL_CURRENT_CENSUS_CHILDREN_RECONCILED',source=pin(HERE/'final-v1/current-universe.json'),before=pin(HERE/'current-ownership-v2.json'),after=pin(HERE/'current-ownership-v3.json'),queue_before=pin(HERE/'remaining-work-queue-v1.json'),queue_after=pin(HERE/'remaining-work-queue-v2.json'),corrections=corrections,supported_identity_delta=0,fragment_status_delta=0,source_changes=0,original_queries=0))
print('Actual current support child lists corrected',len(corrections),'with no ownership-status changes')
