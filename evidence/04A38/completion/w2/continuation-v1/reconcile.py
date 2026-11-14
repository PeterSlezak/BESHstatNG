"""Recount live identities; retain OPEN whenever whole-body proof is absent."""
import csv
from common import *
import verify_p0,verify_p1_v5
def source_fragment(row):
 raw=(ROOT/row['path']).read_bytes();assert sha(raw)==row['source_sha256'];s=raw.decode('utf-8-sig').encode('utf-16-le')
 return s[row['span_start']*2:(row['span_start']+row['span_length'])*2].decode('utf-16-le')
def main():
 stage='p1-v5';p0=verify_p0.inspect();p1=verify_p1_v5.inspect();save('p1/independent-proof.json',p1)
 current=json.loads((HERE/('current-universe-'+stage+'.json')).read_bytes());prior=json.loads((W2/'current-universe-profiles-v1.json').read_bytes())
 old_closed=json.loads((W2/'independent-delegated-proof-v2.json').read_bytes());carried=[];not_carried=[]
 for row in old_closed['rows']:
  if row['status'].startswith('OPEN'):continue
  proof=[]
  for config in ['Debug','Release']:
   old={r['method']:r for r in json.loads((W2/('method-bodies-v2-current-'+config+'.json')).read_bytes())}
   now={r['method']:r for r in json.loads((HERE/'inspection'/stage/('MethodBodies38-'+config+'.json')).read_bytes())}
   ids=[row['method'],row['target']];owner=row['method'].split('(')[0].rsplit('.',1)[0];ctor=owner+'..cctor()'
   if ctor in old:ids.append(ctor)
   exact=all(i in old and i in now and old[i]==now[i] for i in ids);proof.append(dict(configuration=config,entire_record_exact=exact,methods=ids))
  (carried if all(r['entire_record_exact'] for r in proof) else not_carried).append(dict(row,continuation_proof=proof))
 save('p1/carried-delegated-proof.json',dict(carried=carried,not_carried=not_carried,rule='Full instruction/locals/exceptions/static-context records unchanged in both fresh assemblies; not just a call edge'))
 # Existing eight operation-reviewed profile preparations remain byte-token identical.
 before=(HERE/'source-before'/('BESHStat.Excel.Common/src/Analysis/UdfModelProfilePreparation38.vb.txt')).read_text(encoding='utf-8-sig')
 now=(ROOT/verify_p1_v5.PROFILE).read_text(encoding='utf-8-sig');old_profile=json.loads((W2/'independent-profile-proof.json').read_bytes())
 for n in ['TryGetGeeAtSpec','AddGeeAtValue','TryGetMmrmAtSpec','AddMmrmAtValue','TryGetMmrmHeaderlessNameValueAtSpec','TryGetMmrmDesignColumnValues','TryGetGeeSpec','TryGetMmrmSpec']:
  assert verify_p0.tokens(verify_p0.member(before,n))==verify_p0.tokens(verify_p0.member(now,n)),n
 # P0 source operations also survive subsequent source moves exactly.
 snapshot=(HERE/'source-after/p0-v2'/('BESHStat.Excel.Common/src/Analysis/UdfModelProfilePreparation38.vb.txt')).read_text(encoding='utf-8-sig')
 for n in ['FormatMmrmAtProfileDescription','TryBuildMmrmAtProfileRowMask']:
  assert verify_p0.tokens(verify_p0.member(snapshot,n))==verify_p0.tokens(verify_p0.member(now,n)),n
 historical=json.loads((W2/'historical-ownership-checkpoint.json').read_bytes())['members']
 proofs={r['original_id']:dict(kind='FULL_DUAL_CONFIGURATION_IL_CARRIED',evidence='p1/carried-delegated-proof.json') for r in carried}
 proofs.update({r['canonical_original_id']:dict(kind='PRESERVED_PRIOR_SOURCE_OPERATIONS',evidence='prior8 Common bodies token-exact to entry; unchanged wrappers and model/cell boundary') for r in old_profile['targets']})
 for suffix in p0['original_ids']:
  id=next(r['canonical_original_id'] for r in historical if r['canonical_original_id'].endswith(suffix));proofs[id]=dict(kind='P0_SOURCE_OPERATIONS',evidence='p0/independent-proof.json')
 proofs.update({r['original_id']:dict(kind=r['disposition'],evidence='p1/independent-proof.json',target=r['portable_target']) for r in p1['proofs']})
 bykey={}
 for r in current['members']:bykey.setdefault(r['signature_key'],[]).append(r)
 ledger=[];closed_current=set()
 for row in historical:
  keys={r.split('@')[0] for r in row['current_ids']}|{row['git_original_id'].split('@')[0]};matches=[m for k in keys for m in bykey.get(k,[])]
  ids=sorted({r['symbol_id'] for r in matches});proof=proofs.get(row['canonical_original_id'])
  if proof:closed_current.update(ids)
  ledger.append(dict(row,current_ids=ids,disposition='PROOF_SUPPORTED_REVIEWED_BODY' if proof else 'OPEN_W2_WHOLE_BODY_OR_BOUNDARY_REVIEW_REQUIRED',proof=proof,identity_note='Explicit relocation to Common; current native method removed' if proof and not ids else row.get('identity_note')))
 # Accessor callbacks inside reviewed wrappers require their own exact restricted shape.
 accessor_rows=[]
 for row in current['members']:
  if row['kind']=='SingleLineFunctionLambdaExpression' and row['parent_member'] in closed_current:
   s=source_fragment(row).strip();pattern=r'Function\((?:cell|row, column)\) (?:Convert\.ToString\(cell\)\.Trim\(\)|TryGetDouble\(arr\(row, column\)\)\.HasValue|h\.(?:FixedEffectNames|FittedDesign|VisitValues))'
   if re.fullmatch(pattern,s):closed_current.add(row['symbol_id']);accessor_rows.append(dict(id=row['symbol_id'],source=s,proof='Pure native accessor/cell interpretation callback in operation-reviewed parent; exact expression'))
 save('historical-ownership.json',dict(status='NOT_READY_04A38',total=len(ledger),closed=sum(bool(r['proof']) for r in ledger),OPEN=sum(not r['proof'] for r in ledger),members=ledger))
 fragments=[dict(r,review_status='COVERED_BY_REVIEWED_BODY' if r['member_id'] in closed_current else 'OPEN_STATEMENT_INLINE_OR_BOUNDARY_REVIEW_REQUIRED') for r in current['fragments']]
 save('current-ownership.json',dict(status='NOT_READY_04A38',members=[dict(r,review_status='REVIEWED_BODY_OR_EXPLICIT_ACCESSOR' if r['symbol_id'] in closed_current else 'OPEN_WHOLE_BODY_REVIEW_REQUIRED') for r in current['members']],support=[dict(r,review_status='OPEN_SUPPORT_BOUNDARY_REVIEW_REQUIRED') for r in current['supports']],fragments=fragments,accessor_proofs=accessor_rows))
 summary=dict(status='NOT_READY_04A38',W2='IN_PROGRESS',W3='NOT_STARTED',historical=dict(total=len(ledger),closed=len(proofs),OPEN=len(ledger)-len(proofs)),current=dict(total=len(current['members']),closed=len(closed_current),OPEN=len(current['members'])-len(closed_current)),support=dict(raw=len(current['supports']),distinct=sum(not r['already_in_primary_population'] for r in current['supports']),fully_reconciled=0),fragments=dict(total=len(fragments),branches=sum(r['is_branch'] for r in fragments),inline=sum(r['is_inline'] for r in fragments),covered_by_reviewed_parent=sum(r['member_id'] in closed_current for r in current['fragments'])),carried_full_IL=len(carried),dropped_full_IL=len(not_carried),source_files=len(current['sources']),exports=268,new_reference_authority_needed=False,remaining_by_family=[dict(path=s['path'],historical_OPEN=sum(not r['proof'] and r['path']==s['path'] for r in ledger)) for s in current['sources']])
 save('ownership-summary.json',summary);print(json.dumps({k:summary[k] for k in ['historical','current','support','fragments','carried_full_IL','dropped_full_IL']}))
if __name__=='__main__':
 import re
 main()
