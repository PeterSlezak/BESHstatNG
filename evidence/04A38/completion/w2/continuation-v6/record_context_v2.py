"""Compute complete CIL retention and real producer/consumer edges."""
from common import *
import re
ALIAS={'BESHStatNG.ExcelCommon.UdfNumericImportPreparation38+_Closure$__8-0':'BESHStatNG.ExcelCommon.UdfNumericImportPreparation38+_Closure$__9-0'}
def normalize(value):
 s=json.dumps(value,sort_keys=True,ensure_ascii=False)
 for a,b in ALIAS.items():s=s.replace(a,b)
 return json.loads(s)
def key(r):return r['method']
def semantic(r):return {k:v for k,v in r.items() if k!='raw_IL_sha256'}
expected={}
def add(owner,names,target):
 for name in names:expected[(owner,name)]=target
N='BESHStatNG.UdfDataImport';P='BESHStatNG.WorksheetFunctions.ParametricUDFs';R='BESHStatNG.WorksheetFunctions.NonparametricUDFs'
NUM='BESHStatNG.ExcelCommon.UdfNumericImportPreparation38';INF='BESHStatNG.ExcelCommon.UdfInferencePreparation38';CORE='BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38'
for name,target in [('TryGetOptionalProbabilityThresholds','TryReadProbabilityThresholdsPrepared'),('TryGetSingleNumericColumn','ReadFiniteColumn'),('TryReadIndependentNumericColumns','TryReadIndependentNumericColumnsPrepared'),('ExtractCompleteNumericMatrixCompleteCases','ExtractCompleteNumericMatrixCompleteCasesPrepared'),('TryReadCompleteNumericMatrixWithHeaders','TryReadCompleteNumericMatrixWithHeadersPrepared'),('ExtractNumericColumnIgnoringNonNumeric','ReadFiniteColumn')]:add(N,[name],(NUM,target))
add(N,['TryReadPairedNumericColumns','ExtractPairedNumericColumnsIgnoringNonNumeric'],(CORE,'RowsToMatrix'))
for name,target in [('RMANOVA1','ApplyRepeatedCorrection'),('ANOVA2_NESTED','NestedTableSelection'),('RMANOVA1_MCP','BuildRepeatedMcpTables'),('TTEST_UNPAIRED','IndependentTTableSelection'),('TTEST_UNPAIRED_NI','MeanNonInferiorityBody'),('TTEST_UNPAIRED_EQUIV','MeanEquivalenceBody')]:add(P,[name],(INF,target))
add(R,['SPEARMAN_RHO','SPEARMAN_P','KENDALL_TAU','KENDALL_P'],(INF,'SplitCorrelationColumns'))
add(R,['KW_STAT','KW_P'],(INF,'KruskalGroupNames'))
add(R,['FRIEDMAN_STAT','FRIEDMAN_P'],(INF,'FriedmanGroupNames'))
add(R,['KW_MCP'],(INF,'BuildKruskalMcpTables'));add(R,['FRIEDMAN_MCP'],(INF,'BuildFriedmanMcpTables'))
allowed=set(expected)|{(NUM,'ExtractCompleteNumericMatrixCompleteCases'),(NUM,'TryReadCompleteNumericMatrixWithHeaders')}
records=[]
for config in ['Debug','Release']:
 old=json.loads((PREVIOUS/('dependency-metadata-v9/current-'+config+'.json')).read_bytes())
 new=json.loads((HERE/('metadata-full/families-v2/current-'+config+'.json')).read_bytes())
 original=json.loads((HERE/('metadata-full/families-v2/original-'+config+'.json')).read_bytes())
 b={key(r):r for r in new['methods']};changed=[];retained=[]
 for before in old['methods']:
  row=normalize(before);after=b[key(row)]
  if semantic(row)!=semantic(after):
   assert (row['declaring_type'],row['member_name']) in allowed,key(row)
   changed.append(dict(owner=row['declaring_type'],member=row['member_name'],before_sha256=sha(json.dumps(semantic(row),sort_keys=True).encode()),after_sha256=sha(json.dumps(semantic(after),sort_keys=True).encode())))
  else:retained.append(row['method'])
 assert len(changed)==26
 newtypes={r['type']:r for r in new['types']}
 for before in old['types']:
  row=normalize(before);assert row==newtypes[row['type']],'Old type/field context changed '+row['type']
 engines=lambda r:r['declaring_type'].startswith(('BESHStatNG.parametric.','BESHStatNG.nonparametric.','BESHStatNG.equivalencetests.','BESHStatNG.Matrix.')) or r['declaring_type']=='BESHStatNG.ExcelDnaSharedApplicationAdapter'
 a={key(r):r for r in original['methods'] if engines(r)};z={key(r):r for r in new['methods'] if engines(r)}
 assert a.keys()==z.keys() and all(semantic(a[k])==semantic(z[k]) for k in a)
 non38=lambda r:r['declaring_type'].startswith('BESHStatNG.ExcelCommon.') and '38' not in r['declaring_type']
 ao={key(r):r for r in original['methods'] if non38(r)};bn={key(r):r for r in new['methods'] if non38(r)}
 assert ao.keys()==bn.keys() and all(semantic(ao[k])==semantic(bn[k]) for k in ao),'Existing typed transport/parser behavior changed'
 edges=[]
 for (owner,name),(target_owner,target_name) in expected.items():
  callers=[r for r in new['methods'] if r['declaring_type']==owner and r['member_name']==name];assert len(callers)==1
  callees=[r for r in new['methods'] if r['declaring_type']==target_owner and r['member_name']==target_name];assert len(callees)==1
  calls=[i for i in callers[0]['instructions'] if i['op'] in ['call','callvirt'] and i['operand']=='METHOD:'+callees[0]['method']]
  assert len(calls)==1,(owner,name,target_name)
  edges.append(dict(owner=owner,member=name,callee=callees[0]['method'],call_instruction=calls[0],caller_method=callers[0]['method'],caller_source_metadata=pin(HERE/('metadata-full/families-v2/current-'+config+'.json'))))
 records.append(dict(configuration=config,retained_complete_v5_methods=len(retained),retained_method_names=retained,authorized_changes=changed,
    retained_v5_types=len(old['types']),engine_and_adapter_methods=len(a),typed_transport_parser_existing_Common_methods=len(ao),
    full_engine_types=[t for t in original['types'] if t['type'].startswith(('BESHStatNG.parametric.','BESHStatNG.nonparametric.','BESHStatNG.equivalencetests.','BESHStatNG.Matrix.')) or t['type']=='BESHStatNG.ExcelDnaSharedApplicationAdapter'],
    actual_edges=edges,assemblies=new['assemblies'],original_assemblies=original['assemblies']))
save('compiled-context-and-consumers.json',dict(status='FULL_CIL_FIELD_CONTEXT_AND_24_REAL_CONSUMERS_PASS',configurations=records,
    exact_generated_type_alias=ALIAS,alias_scope='Single literal enclosing-method ordinal relocation; every constructor, lambda instruction, parameter, field and caller context rechecked after that exact replacement only',original_queries=0))
print(json.dumps([dict(configuration=r['configuration'],retained=r['retained_complete_v5_methods'],changed=len(r['authorized_changes']),engines=r['engine_and_adapter_methods'],typed_Common=r['typed_transport_parser_existing_Common_methods'],edges=len(r['actual_edges'])) for r in records]))
