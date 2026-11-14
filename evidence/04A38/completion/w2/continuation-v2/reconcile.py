"""Refresh exact source identities, retaining OPEN outside independently proved bodies."""
from common import *
from verify_slice_v2 import tokens
u=json.loads((HERE/'current-universe-b1-v1.json').read_bytes());old=json.loads((PREVIOUS/'current-ownership.json').read_bytes());hist=json.loads((PREVIOUS/'historical-ownership.json').read_bytes())['members']
proofs={};carried=[];old39=json.loads((W2/'independent-delegated-proof-v2.json').read_bytes())['rows']
for row in old39:
 if row['status'].startswith('OPEN'):continue
 checks=[]
 for c in ['Debug','Release']:
  former={r['method']:r for r in json.loads((W2/('method-bodies-v2-current-'+c+'.json')).read_bytes())};now={r['method']:r for r in json.loads((HERE/'inspection/b1-v1'/('MethodBodies38-'+c+'.json')).read_bytes())}
  ids=[row['method'],row['target']];ctor=row['method'].split('(')[0].rsplit('.',1)[0]+'..cctor()'
  if ctor in former:ids.append(ctor)
  exact=all(i in former and i in now and former[i]==now[i] for i in ids);assert exact,'Carried39 body/target/static context changed '+row['method'];checks.append(dict(configuration=c,methods=ids,entire_record_exact=True))
 carried.append(dict(row,continuation_v2=checks))
assert len(carried)==39
save('carried-dual-IL39.json',dict(rows=carried,status='ALL39_FULL_RECORDS_AND_TARGET_STATIC_CONTEXT_RECHECKED'))
# Existing proof-supported bodies are carried only with source/body-token identities.
oldmembers={r['symbol_id']:r for r in old['members']};newbykey={}
for r in u['members']:newbykey.setdefault(r['signature_key'],[]).append(r)
priorproofs=[]
for r in hist:
 if not r['proof']:continue
 mapped=[]
 for id in r['current_ids']:
  if id not in oldmembers:continue
  former=oldmembers[id];matches=newbykey.get(former['signature_key'],[])
  assert len(matches)==1 and former['body_token_sha256']==matches[0]['body_token_sha256'],'Prior reviewed native body changed '+id
  mapped.append(matches[0]['symbol_id'])
 proofs[r['canonical_original_id']]=dict(kind='CARRIED_OPERATION_OR_FULL_IL_PROOF',prior=r['proof'],source_token_exact=True,current_ids=mapped)
 priorproofs.append(dict(original_id=r['canonical_original_id'],current_ids=mapped))
assert len(priorproofs)==60
# Keep relocated portable source byte-identical for all old non-IL reviewed profile/report/import bodies.
oldtrans=json.loads((PREVIOUS/'source-transitions-p1-v5.json').read_bytes())['files']
unchanged=[]
for r in oldtrans:
 p=r['after']['path']
 if p.startswith('BESHStat.Excel.Common/src/'):
  assert pin(ROOT/p)==r['after'],'Prior operation-reviewed portable file changed '+p
  unchanged.append(r['after'])
save('carried-source-operation60.json',dict(prior_supported=60,native_rows=priorproofs,unchanged_portable_sources=unchanged,full_IL39='carried-dual-IL39.json',relocated_helpers='Prior8 model profile parsers plus P0 description/mask and P1 imported/reported bodies remain byte-identical; their historical original IDs retained.'))
new=json.loads((HERE/'independent-slice-proof.json').read_bytes())['proofs'];cache=json.loads((HERE/'host-cache-proof.json').read_bytes())
for r in new:proofs[r['original_id']]=dict(kind='V2_SOURCE_OPERATIONS_AND_COMPILED_CONSUMER',evidence='independent-slice-proof.json',target=r['portable_target'])
for r in cache['methods']+cache['initializers']:proofs[r['original_id']]=dict(kind='V2_EXACT_SOURCE_AND_FULL_DUAL_CONFIGURATION_CONTEXT',evidence='host-cache-proof.json',disposition=r['disposition'])
closed=set();ledger=[]
for r in hist:
 keys={id.split('@')[0] for id in r['current_ids']}|{r['git_original_id'].split('@')[0]};matches=[m for key in keys for m in newbykey.get(key,[])];ids=sorted({m['symbol_id'] for m in matches});p=proofs.get(r['canonical_original_id'])
 if p:closed.update(ids)
 ledger.append(dict(r,current_ids=ids,proof=p,disposition='PROOF_SUPPORTED_REVIEWED_BODY' if p else 'OPEN_W2_WHOLE_BODY_OR_BOUNDARY_REVIEW_REQUIRED'))
# Carry the previously restricted accessor proofs only if source tokens match.
for oldrow in old['members']:
 if oldrow['review_status'].startswith('OPEN'):continue
 matches=newbykey.get(oldrow['signature_key'],[])
 if len(matches)==1 and oldrow['body_token_sha256']==matches[0]['body_token_sha256']:closed.add(matches[0]['symbol_id'])
# The new capturing Levene callback is not hidden behind its parent.
accessors=[]
for r in u['members']:
 if r['kind']=='SingleLineFunctionLambdaExpression' and r['parent_member'] in closed:
  source=(ROOT/r['path']).read_bytes().decode('utf-8-sig').encode('utf-16-le')[r['span_start']*2:(r['span_start']+r['span_length'])*2].decode('utf-16-le')
  if source.strip()=='Function() Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.AsString(arg)':
   closed.add(r['symbol_id']);accessors.append(dict(id=r['symbol_id'],source=source,proof='Exactly one actual native AsString invocation after parent reset, same captured Object reference; expanded metadata captures generated constructor/field/lambda; actual throwing/side-effecting delegate tests in both configurations'))
support=json.loads((HERE/'support-reconciliation.json').read_bytes());fragments=[dict(r,review_status='COVERED_BY_REVIEWED_BODY' if r['member_id'] in closed else 'OPEN_STATEMENT_INLINE_OR_BOUNDARY_REVIEW_REQUIRED') for r in u['fragments']]
save('historical-ownership.json',dict(status='NOT_READY_04A38',total=len(ledger),closed=sum(bool(r['proof']) for r in ledger),OPEN=sum(not r['proof'] for r in ledger),members=ledger))
save('current-ownership.json',dict(status='NOT_READY_04A38',members=[dict(r,review_status='REVIEWED_BODY_OR_EXPLICIT_ACCESSOR' if r['symbol_id'] in closed else 'OPEN_WHOLE_BODY_REVIEW_REQUIRED') for r in u['members']],support=support['rows'],fragments=fragments,accessor_proofs=accessors))
summary=dict(status='NOT_READY_04A38',W2='IN_PROGRESS',W3='NOT_STARTED',historical=dict(total=len(ledger),closed=sum(bool(r['proof']) for r in ledger),OPEN=sum(not r['proof'] for r in ledger)),current=dict(total=len(u['members']),closed=len(closed),OPEN=len(u['members'])-len(closed)),support={k:support[k] for k in ['raw','distinct','raw_fully_reconciled','distinct_fully_reconciled']},fragments=dict(total=len(fragments),branches=sum(r['is_branch'] for r in fragments),inline=sum(r['is_inline'] for r in fragments),covered_by_reviewed_parent=sum(r['member_id'] in closed for r in fragments)),carried_full_IL=39,new_cache_host_methods=len(cache['methods']),new_primary_initializers=len(cache['initializers']),new_source_operation_bodies=len(new),exports=268,source_files=len(u['sources']),owner_decision_needed=False,new_original_queries=0)
save('ownership-summary.json',summary);print(json.dumps(summary))
