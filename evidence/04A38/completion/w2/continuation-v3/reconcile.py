"""Complete population retained; only source/IL proved additions become reviewed."""
from common import *
from collections import defaultdict
from verify_names import tokens
u=json.loads((HERE/'current-universe-names-v3.json').read_bytes());old=json.loads((PREVIOUS/'current-ownership-v2.json').read_bytes());h=json.loads((PREVIOUS/'historical-ownership-v2.json').read_bytes())['members'];bykey=defaultdict(list)
for r in u['members']:bykey[r['signature_key']].append(r)
# The39 old whole IL proofs remain actual full-record comparisons, including ctor context.
records={c:{r['method']:r for r in json.loads((HERE/'inspection/names-v3'/('MethodBodies38-'+c+'.json')).read_bytes())} for c in ['Debug','Release']};former={c:{r['method']:r for r in json.loads((W2/('method-bodies-v2-current-'+c+'.json')).read_bytes())} for c in ['Debug','Release']};carried=[]
for r in json.loads((W2/'independent-delegated-proof-v2.json').read_bytes())['rows']:
 if r['status'].startswith('OPEN'):continue
 checks=[]
 for c in ['Debug','Release']:
  ids=[r['method'],r['target']];ctor=r['method'].split('(')[0].rsplit('.',1)[0]+'..cctor()'
  if ctor in former[c]:ids.append(ctor)
  assert all(i in records[c] and former[c][i]==records[c][i] for i in ids),'Carried source/target/context needs explicit review '+r['method']
  checks.append(dict(configuration=c,methods=ids,entire_record_exact=True))
 carried.append(dict(r,C3=checks))
assert len(carried)==39;save('carried39.json',dict(rows=carried))
proofs={};oldbyid={r['symbol_id']:r for r in old['members']}
for r in h:
 if not r['proof']:continue
 for id in r['current_ids']:
  if id not in oldbyid:continue
  before=oldbyid[id];matches=[m for m in bykey[before['signature_key']] if m['body_token_sha256']==before['body_token_sha256'] and (m['parent_member'] or '').split('@')[0]==(before['parent_member'] or '').split('@')[0]];assert len(matches)==1,'Prior reviewed body/lambda changed '+id
 proofs[r['canonical_original_id']]=dict(kind='CARRIED_C2_FULL_SOURCE_OR_CIL_PROOF',prior=r['proof'],native_source_token_exact=True)
assert len(proofs)==120
for name in ['independent-names-proof.json','independent-unchanged-proof.json']:
 for r in json.loads((HERE/name).read_bytes())['proofs']:proofs[r['original_id']]=dict(kind='C3_SOURCE_OPERATIONS_AND_COMPILED_CONTEXT',evidence=name,target=r['target'])
ledger=[];closed=set()
for r in h:
 keys={v.split('@')[0] for v in r['current_ids']}|{r['git_original_id'].split('@')[0]};ids=sorted({m['symbol_id'] for k in keys for m in bykey.get(k,[])})
 p=proofs.get(r['canonical_original_id'])
 if p:closed.update(ids)
 ledger.append(dict(r,current_ids=ids,proof=p,disposition='PROOF_SUPPORTED_REVIEWED_BODY' if p else 'OPEN_W2_WHOLE_BODY_OR_BOUNDARY_REVIEW_REQUIRED'))
# Carry reviewed duplicate-signature lambdas only through body AND parent identity.
for r in old['members']:
 if r['review_status'].startswith('OPEN'):continue
 matches=[m for m in bykey.get(r['signature_key'],[]) if m['body_token_sha256']==r['body_token_sha256'] and (m['parent_member'] or '').split('@')[0]==(r['parent_member'] or '').split('@')[0]];assert len(matches)==1
 closed.add(matches[0]['symbol_id'])
parent=next(r for r in u['members'] if r['path'].endswith('UdfDataImportFormula.vb') and r['name']=='GetVariableNames');lambdas=[r for r in u['members'] if 'LambdaExpression' in r['kind'] and r['path']==parent['path'] and r['span_start']>=parent['span_start'] and r['span_start']+r['span_length']<=parent['span_start']+parent['span_length']];assert len(lambdas)==4
accessors=[]
for r in lambdas:
 closed.add(r['symbol_id']);accessors.append(dict(id=r['symbol_id'],parent=r['parent_member'],historical_parent=next(v['canonical_original_id'] for v in ledger if v['name']=='GetVariableNames' and v['path']==parent['path']),source_span=dict(start=r['span_start'],length=r['span_length']),source_body_token_sha256=r['body_token_sha256'],proof='Exact complete host-body reconstruction in verify_names includes each missing/text/acquisition/cell callback, actual compiled nested closures; current exceptional/mutation/bound-normalization production tests'))
# Support identities can move their spans; declaration and owner remain exact.
old_support=defaultdict(list)
for r in old['support']:old_support[(r['path'],r['kind'],r['owner'],r['declaration'])].append(r)
support=[]
for r in u['supports']:
 a=old_support[(r['path'],r['kind'],r['owner'],r['declaration'])];assert len(a)==1
 former=a[0];row=dict(r,disposition=former['disposition'],fully_reconciled=former['fully_reconciled'],reason=former['reason'])
 if r['kind']=='PropertyStatement':row['proof']=former['proof'];row['fresh_metadata']='metadata-v5/names-v3 both configurations; accessors/fields/ctor comparisons independently rerun'
 if r['kind']=='ModuleBlock':row['children']=[m['symbol_id'] for m in u['members'] if m['path']==r['path'] and m['span_start']>=r['span_start'] and m['span_start']+m['span_length']<=r['span_start']+r['span_length']];row['fully_reconciled']=False
 if r['kind']=='FieldDeclaration':row['aliases']=[m['symbol_id'] for m in u['members'] if m['path']==r['path'] and m['span_start']>=r['span_start'] and m['span_start']+m['span_length']<=r['span_start']+r['span_length']]
 support.append(row)
fragments=[dict(r,review_status='COVERED_BY_REVIEWED_BODY' if r['member_id'] in closed else 'OPEN_STATEMENT_INLINE_OR_BOUNDARY_REVIEW_REQUIRED') for r in u['fragments']]
save('historical-ownership.json',dict(status='NOT_READY_04A38',total=len(ledger),closed=sum(bool(r['proof']) for r in ledger),OPEN=sum(not r['proof'] for r in ledger),members=ledger))
save('current-ownership.json',dict(status='NOT_READY_04A38',members=[dict(r,review_status='REVIEWED_BODY_OR_EXPLICIT_CALLBACK' if r['symbol_id'] in closed else 'OPEN_WHOLE_BODY_REVIEW_REQUIRED') for r in u['members']],support=support,fragments=fragments,callback_proofs=accessors,portable_contracts=['UdfNamePreparation38:3policies/privateconstructor','UdfVariableNameRange38:typed dimension/accessor contract/readonly instance storage; full current metadata and source in names proof']))
summary=dict(status='NOT_READY_04A38',W2='IN_PROGRESS',W3='NOT_STARTED',historical=dict(total=len(ledger),closed=sum(bool(r['proof']) for r in ledger),OPEN=sum(not r['proof'] for r in ledger)),current=dict(total=len(u['members']),closed=len(closed),OPEN=len(u['members'])-len(closed)),support=dict(raw=len(support),distinct=sum(not r['already_in_primary_population'] for r in support),distinct_closed=sum(r['fully_reconciled'] and not r['already_in_primary_population'] for r in support),distinct_OPEN=sum(not r['fully_reconciled'] and not r['already_in_primary_population'] for r in support)),fragments=dict(total=len(fragments),branches=sum(r['is_branch'] for r in fragments),inline=sum(r['is_inline'] for r in fragments),covered=sum(r['member_id'] in closed for r in fragments)),new_historical_supported=len(proofs)-120,new_name_callbacks=4,carried39=len(carried),source_files=len(u['sources']),exports=268,new_original_queries=0,owner_decision_needed=False)
save('ownership-summary.json',summary);print(json.dumps(summary))
