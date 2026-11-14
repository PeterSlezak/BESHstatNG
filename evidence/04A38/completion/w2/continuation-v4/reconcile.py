from common import *
from collections import defaultdict
u=json.loads((HERE/'current-universe-parametric-v4.json').read_bytes());old=json.loads((PREVIOUS/'current-ownership.json').read_bytes());hist=json.loads((PREVIOUS/'historical-ownership.json').read_bytes())['members'];proof=json.loads((HERE/'independent-slice-proof.json').read_bytes())['result'];new={r['original_id']:r for r in proof['proofs']};bykey=defaultdict(list)
for r in u['members']:bykey[r['signature_key']].append(r)
closed=set();ledger=[]
for h in hist:
 keys={id.split('@')[0] for id in h['current_ids']}|{h['git_original_id'].split('@')[0]};ids=sorted({r['symbol_id'] for k in keys for r in bykey[k]});p=h['proof']
 if h['canonical_original_id'] in new:p=dict(kind='C4_COMPLETE_SOURCE_CIL_BOUNDARY_CONTEXT_PROOF',evidence='independent-slice-proof.json',detail=new[h['canonical_original_id']])
 if p:closed.update(ids)
 ledger.append(dict(h,current_ids=ids,proof=p,disposition='PROOF_SUPPORTED_REVIEWED_BODY' if p else 'OPEN_W2_WHOLE_BODY_OR_BOUNDARY_REVIEW_REQUIRED',partial_implementation='C4 real Common report consumer; import/output dependency whole-body closure remains OPEN' if h['name'] in ['ANOVA1','ANOVA1_WELCH','ANOVA1_MCP'] else None))
for r in old['members']:
 if r['review_status'].startswith('OPEN'):continue
 matches=[m for m in bykey[r['signature_key']] if m['body_token_sha256']==r['body_token_sha256'] and (m['parent_member'] or '').split('@')[0]==(r['parent_member'] or '').split('@')[0]];assert len(matches)==1,'Retained reviewed parent/body changed';closed.add(matches[0]['symbol_id'])
support_old=defaultdict(list)
for r in old['support']:support_old[(r['path'],r['kind'],r['owner'],r['declaration'])].append(r)
support=[]
for r in u['supports']:
 a=support_old[(r['path'],r['kind'],r['owner'],r['declaration'])];assert len(a)==1;prior=a[0];r=dict(r,disposition=prior['disposition'],fully_reconciled=prior['fully_reconciled'],reason=prior['reason'])
 if r['kind']=='PropertyStatement':r.update(proof=prior['proof'],fresh_metadata='C4 metadata-v6 current/original; retained exact full field/ctor/accessor context')
 if r['kind'] in ['ModuleBlock','ClassBlock']:r.update(fully_reconciled=False,children=[m['symbol_id'] for m in u['members'] if m['path']==r['path'] and m['span_start']>=r['span_start'] and m['span_start']+m['span_length']<=r['span_start']+r['span_length']])
 if r['kind']=='FieldDeclaration':r['aliases']=[m['symbol_id'] for m in u['members'] if m['path']==r['path'] and m['span_start']>=r['span_start'] and m['span_start']+m['span_length']<=r['span_start']+r['span_length']]
 support.append(r)
fragments=[dict(r,review_status='COVERED_BY_REVIEWED_BODY' if r['member_id'] in closed else 'OPEN_STATEMENT_INLINE_OR_BOUNDARY_REVIEW_REQUIRED') for r in u['fragments']]
save('historical-ownership.json',dict(status='NOT_READY_04A38',total=len(ledger),closed=sum(bool(r['proof']) for r in ledger),OPEN=sum(not r['proof'] for r in ledger),members=ledger))
save('current-ownership.json',dict(status='NOT_READY_04A38',members=[dict(r,review_status='REVIEWED_BODY_OR_EXPLICIT_CALLBACK' if r['symbol_id'] in closed else 'OPEN_WHOLE_BODY_REVIEW_REQUIRED') for r in u['members']],support=support,fragments=fragments,callback_proofs=old['callback_proofs'],new_portable_contracts=['TryGetSquareMatrixDimensions and TryInvertPreparedSquareMatrix: no caught engine failures; real Windows catch owns Framework Err','ParametricUdfReports38:3 existing-engine orchestration/report contracts; no duplicated ANOVA calculation']))
summary=dict(status='NOT_READY_04A38',W2='IN_PROGRESS',W3='NOT_STARTED',historical=dict(total=len(ledger),closed=sum(bool(r['proof']) for r in ledger),OPEN=sum(not r['proof'] for r in ledger)),current=dict(total=len(u['members']),closed=len(closed),OPEN=len(u['members'])-len(closed)),support=dict(raw=len(support),distinct=sum(not r['already_in_primary_population'] for r in support),distinct_closed=sum(r['fully_reconciled'] and not r['already_in_primary_population'] for r in support),distinct_OPEN=sum(not r['fully_reconciled'] and not r['already_in_primary_population'] for r in support)),fragments=dict(total=len(fragments),branches=sum(r['is_branch'] for r in fragments),inline=sum(r['is_inline'] for r in fragments),covered=sum(r['member_id'] in closed for r in fragments)),new_historical_supported=len(new),carried39=39,source_files=len(u['sources']),exports=268,new_original_queries=0,owner_decision_needed=False)
save('ownership-summary.json',summary);print(json.dumps(summary))
