"""Reconcile completed extraction edges without closing unreviewed members.

This progress ledger deliberately retains OPEN entries. A reachable portable
call is evidence of consumption, not proof that every other statement migrated.
"""
from collections import defaultdict,Counter
import csv,json,re,sys
from preflight import ROOT,HERE,save,pin

def main():
 label=sys.argv[1];entry=json.loads((HERE/'entry-members.json').read_bytes())
 folder=HERE/'compatibility'/label/'Debug/inspection'
 closure=json.loads((folder/'consumer-closure.json').read_bytes());compiled=json.loads((folder/'compiled.json').read_bytes())
 graph={r['method']:r for r in closure['methods']};consumers=defaultdict(set);by_name=defaultdict(list)
 for row in closure['methods']:by_name[row['method'].split('(')[0].replace('+','.')].append(row)
 for export in compiled['exports']:
  todo=[export['method']];seen=set()
  while todo:
   key=todo.pop()
   if key in seen:continue
   seen.add(key);consumers[key].add(export['effective_excel_name'])
   todo.extend(c['target'] for c in graph.get(key,{}).get('calls',[]) if c['assembly']=='BESHStatNG')
 with (HERE/'udf-member-preparation-plan-v1.csv').open(encoding='utf-8',newline='') as f:plan={r['current_member_id']:r for r in csv.DictReader(f)}
 mappings={}
 for file,idfield in [('numeric-preparation-transition-v1.json','original_member_id'),('option-preparation-transition-v2.json','original_member_id'),('report-preparation-transition-v1.json','source_member_id'),('metadata-preparation-transition-v1.json','source_member_id')]:
  for row in json.loads((HERE/file).read_bytes())['members']:mappings[row[idfield].split('@')[0]]=file
 extra={
  'UdfDataImport':{'FindLastNonBlankRow','HasNumericColumnHeader','HasBinaryColumnHeader','HasNumericMatrixHeader','LooksLikeHeaderRow','LooksLikeSingleColumnHeader','TryExtractNumericGroupsFromMatrix','ExtractCompleteNumericMatrixCompleteCases','TryReadCompleteNumericMatrixWithHeaders','TryReadStrictNumericVectorFromSingleColumn','TryReadStrictNumericVectorFromSingleRow','TryParseAlpha','GuessPositiveClass'},
  'ExcelArgNumeric':{'GetOptionalBool','GetOptionalInt','GetOptionalDouble','TryGetBinary01','TryGetInteger','TryGetFiniteDoubleFlexible','TryGetOptionalPositiveInteger'},
  'PropensityScoreUDFs':{'NormalizeFitOptions','ApplyOptionText','ApplyOption','EnsureCoarseningSpec','ParseEstimand'},
  'AssumptionsUDFs':{'BuildTwoValueTable'},
  'UdfOutputTables':{'PrepareResultTableForUdf','IsPValueLabel','PrepareWrappedResultTableForUdf','PrepareExistingObjectTableForUdf','ClampProb'},
  'ParametricUDFs':{'StackWithBlankRow'},
  'GEEUDFs':{'AverageGeeFittedDesignRowForLsmProfile'},
  'MixedModelUDFs':{'AverageFittedDesignRowForLsmProfile'},
  'NonparametricUDFs':{'ParseKwType','ParseFriedmanType'},
 }
 records=[];edges=[]
 for member in entry['members']:
  old=plan[member['symbol_id']];bindings=json.loads(old['compiled_bindings']);current=[graph[k] for k in bindings if k in graph]
  if member['kind'] in {'FunctionBlock','SubBlock','GetAccessorBlock'}:assert len(current)==len(bindings) and current,member['symbol_id']
  calls=[dict(windows_method=b['method'],**c) for b in current for c in b['calls'] if c['assembly'] in {'BESHStat.Core','BESHStat.Excel.Common'}]
  ported=member['signature_key'] in mappings or member['name'] in extra.get(member['containing_type'].split('.')[-1],set())
  if member['name']=='PrepareResultTableForUdf' and 'table As ResultTable' not in member['signature']:ported=False
  if ported:
   assert calls,('Unconsumed purported extraction',member['symbol_id'])
   status='PORTED_CONSUMER_EDGE_VERIFIED_REMAINING_BODY_REVIEW_REQUIRED'
  elif old['completion_state'].startswith('HOST_') or old['completion_state'].startswith('EXISTING_'):
   status='ENTRY_BOUNDARY_CLASSIFICATION_RETAINED_FINAL_REVIEW_PENDING'
  else:status='OPEN_REMAINING_MEMBER_BEHAVIOR_REVIEW'
  real=sorted(set().union(*(consumers[b['method']] for b in current))) if current else []
  record=dict(original_member_id=member['symbol_id'],historical_member_id=old['historical_member_id'],path=member['path'],signature=member['signature'],kind=member['kind'],
   progress=status,portable_source_transition=mappings.get(member['signature_key'],''),windows_bindings=json.dumps(bindings),registered_consumers=json.dumps(real),portable_edges=json.dumps(calls),
   predecessor_plan=old['completion_state'],ready_disposition='NOT_CLOSED')
  records.append(record)
  edges.extend(dict(original_member_id=member['symbol_id'],registered_consumers=json.dumps(real),**edge) for edge in calls)
 target=HERE/('ownership-progress-'+label+'-v2.csv')
 with target.open('x',encoding='utf-8',newline='') as f:
  writer=csv.DictWriter(f,fieldnames=list(records[0]));writer.writeheader();writer.writerows(records)
 save('ownership-progress-'+label+'-v2.json',dict(status='NOT_READY_04A38',entry_members=len(records),historical_members=sum(r['historical_member_id'].startswith('BESHstatNG/') for r in records),
  additional_accepted_members=sum(r['historical_member_id']=='ADDITIONAL_ACCEPTED_WINDOWS_COERCION_CALLBACK' for r in records),
  classifications=dict(Counter(r['progress'] for r in records)),final_dispositions_closed=0,
  note='A completed, tested delegation is not an all-statement ownership certification. These progress entries cannot authorize READY. Inline lambdas/property/supporting declarations still require final split/merge reconciliation.',
  ledger=pin(target.relative_to(ROOT).as_posix()),compiled_source=pin((folder/'consumer-closure.json').relative_to(ROOT).as_posix())))
 print('Progress ledger',dict(Counter(r['progress'] for r in records)),'; no false ownership closure')

if __name__=='__main__':main()
