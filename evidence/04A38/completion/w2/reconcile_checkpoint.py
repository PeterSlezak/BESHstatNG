"""Publish exact partial ownership, never fabricate whole-batch closure."""
from capture_current import *
from verify_profile_slice import inspect as profile
from verify_delegated_bodies_v2 import inspect as delegated

def save(n,v):
 with (HERE/n).open('x',encoding='utf-8') as f:json.dump(v,f,indent=2,ensure_ascii=False)

def main():
 p=profile();d=delegated();save('independent-profile-proof.json',p);save('independent-delegated-proof-v2.json',d)
 old=ROOT/'evidence/04A38/completion/w1';historical=json.loads((old/'member-behavior-evidence-target-matrix-v3.json').read_bytes())['members']
 current=json.loads((HERE/'current-universe-profiles-v1.json').read_bytes());prior=json.loads((old/'current-universe.json').read_bytes())
 closed={r['canonical_original_id']:r for r in p['targets']}
 closed.update({r['original_id']:r for r in d['rows'] if not r['status'].startswith('OPEN')});assert len(closed)==47
 bykey={}
 for row in current['members']:bykey.setdefault(row['signature_key'],[]).append(row)
 ledger=[]
 for row in historical:
  ids=[]
  for original_current_id in row['current_ids']:ids.extend(r['symbol_id'] for r in bykey.get(original_current_id.split('@')[0],[]))
  proof=closed.get(row['canonical_original_id']);ledger.append(dict(canonical_original_id=row['canonical_original_id'],path=row['path'],name=row['name'],git_original_id=row['git_original_id'],current_ids=sorted(set(ids)),
   disposition='VERIFIED_PORTABLE_BODY' if proof else 'OPEN_W2_WHOLE_BODY_OR_BOUNDARY_REVIEW_REQUIRED',proof=proof,
   identity_note='Removed private Add/headerless bodies relocate to Common; DTO initializer properties relocate to generic portable bases; unresolved mappings remain OPEN, never HOST_ONLY' if not ids else None))
 save('historical-ownership-checkpoint.json',dict(status='NOT_READY_04A38',historical_total=len(ledger),closed=len(closed),OPEN=len(ledger)-len(closed),members=ledger))
 current_closed={r['current_id'] for r in d['rows'] if not r['status'].startswith('OPEN')}
 profile_names={r['name'] for r in p['targets']}
 current_closed.update(r['symbol_id'] for r in current['members'] if r['kind']=='FunctionBlock' and r['name'] in profile_names and r['path'] in ['BESHstatNG/src/ExcelUDFs/Regression/GeeUDFs.vb','BESHstatNG/src/ExcelUDFs/Regression/MixedModelUDFs.vb','BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImportMixedModel.vb'])
 parent_ids={r['symbol_id'] for r in current['members'] if r['kind']=='FunctionBlock' and r['name'] in ['TryBuildMmrmAtProfileRowMask','TryGetMmrmLsmEstimateAtSpec','TryGetMmrmLsmEstimateSpec']}
 current_closed.update(r['symbol_id'] for r in current['members'] if r['kind']=='SingleLineFunctionLambdaExpression' and r['parent_member'] in parent_ids)
 save('current-member-support-fragment-checkpoint.json',dict(status='NOT_READY_04A38',members=[dict(row,review_status='VERIFIED_SOURCE_BODY_OR_ACCESSOR_CALLBACK' if row['symbol_id'] in current_closed else 'OPEN_W2_WHOLE_BODY_REVIEW_REQUIRED') for row in current['members']],
  support=[dict(row,review_status='OPEN_SUPPORT_BOUNDARY_REVIEW_REQUIRED') for row in current['supports']],
  fragments=[dict(row,review_status='COVERED_BY_VERIFIED_WHOLE_BODY' if row['member_id'] in current_closed else 'OPEN_STATEMENT_INLINE_OR_BOUNDARY_REVIEW_REQUIRED') for row in current['fragments']]))
 changes=[]
 for path in ['BESHstatNG/src/ExcelUDFs/Regression/GeeUDFs.vb','BESHstatNG/src/ExcelUDFs/Regression/MixedModelUDFs.vb','BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImportMixedModel.vb','BESHStat.Core/src/WorksheetFunctions/UdfProfilePreparation38.vb']:
  changes.append(dict(path=path,before=pin(HERE/'source-before'/path),after=pin(ROOT/path),reason='Authorized04A38 profile/spec/grouping/saved-row preparation extraction; no statistical policy change'))
 for path in ['BESHStat.Excel.Common/src/Analysis/UdfModelProfilePreparation38.vb','BESHStat.Core.Tests/src/UdfRowMaskPreparation38Tests.vb','BESHStat.Excel.Common.Tests/src/UdfModelProfilePreparation38Tests.vb']:
  changes.append(dict(path=path,before=None,after=pin(ROOT/path),reason='New portable consumed contract or focused tests'))
 save('source-transitions.json',dict(status='AUTHORIZED_W2_TRANSITIONS',files=changes,whole_body_closure_not_inferred_from_compilation=True,source_before_private_inventory=pin(HERE/'source-before-inventory.json')))
 summary=dict(status='NOT_READY_04A38',historical=dict(total=704,whole_body_closed=47,OPEN=657),current=dict(total=len(current['members']),whole_body_closed=len(current_closed),OPEN=len(current['members'])-len(current_closed)),
  support=dict(raw=len(current['supports']),overlap=sum(r['already_in_primary_population'] for r in current['supports']),distinct=sum(not r['already_in_primary_population'] for r in current['supports']),fully_reconciled=0),
  fragments=dict(total=len(current['fragments']),branches=sum(r['is_branch'] for r in current['fragments']),inline=sum(r['is_inline'] for r in current['fragments']),covered_by_verified_parent=sum(r['member_id'] in current_closed for r in current['fragments'])),
  current_reallocation='719 -3 private methods relocated to Common -8 DTO property initializers relocated to portable bases +4 explicit accessor lambdas =712. Supporting347 -22 DTO properties =325 raw; eight overlap removed from primary, leaving303 distinct (317-14).',
  actual_new_portable_seams=['GEE/MMRM family-specific at grammar and duplicate selections','GEE/MMRM weighted specification preparation including GEE intercept-role exclusion','MMRM scalar fitted-column grouping and Intercept rejection','Core caller-owned saved-row mask filling with partial ByRef write preservation'],
  full_export_parity=dict(Debug=268,Release=268),frozen_preedit_exact_per_configuration=28966,late_B_to_G_exact_per_configuration=26,late_A='Previous immutable432 preserved; fresh post-W2 replay pending',
  W2='IN_PROGRESS_NOT_COMPLETE',W3='NOT_STARTED',new_reference_authority_needed=False,
  earliest_remaining_body='BESHstatNG/src/ExcelUDFs/Regression/MixedModelUDFs.vb::FormatMmrmAtProfileDescription and dependent mask-body closure; continue profile application/report preparation then remaining W1 family schedule',
  remaining_families=[dict(path=r['path'],members=sum(x['path']==r['path'] for x in ledger),OPEN=sum(x['path']==r['path'] and x['disposition'].startswith('OPEN') for x in ledger)) for r in current['sources']])
 save('ownership-summary.json',summary);print(json.dumps({k:summary[k] for k in ['historical','current','support','fragments','W2','W3']}))
if __name__=='__main__':main()
