"""Materialize the approved432 proposal, never expected result values."""
from datetime import datetime,timezone
import json,struct
from prepare_original import ROOT,HERE,BASE,sha,pin,save

def cell(value):
 if value is None:return dict(kind='Nothing')
 if isinstance(value,str):return dict(kind='String',value=value)
 return dict(kind='Double',bits=struct.pack('>d',float(value)).hex().upper())
def matrix(rows):
 columns=len(rows[0]) if rows else 0
 assert all(len(row)==columns for row in rows)
 return dict(kind='Array',element_type='System.Object',rank=2,lower_bounds=[0,0],lengths=[len(rows),columns],upper_bounds=[len(rows)-1,columns-1],
  values=[value if isinstance(value,dict) else cell(value) for row in rows for value in row],input_row_ordinals=list(range(len(rows))),input_column_ordinals=list(range(columns)),physical_worksheet_rows='UNKNOWN')
def argument(name,value):return dict(name=name,value=value)

def main():
 assert (HERE/'owner-authorization.json').exists() and (HERE/'original-source-authenticity.json').exists()
 proposal=json.loads((ROOT/'evidence/04A38/continuation/v1/proposed-missing-case-matrix.json').read_bytes());assert len(proposal['cases'])==432
 records=[]
 for original in proposal['cases']:
  row=dict(case_id=original['case_id'],family=original['family'],culture='' if original['culture']=='Invariant' else 'sk-SK',parameters=original['parameters'],
   provenance='LATE_ARCHIVED_ENTRY_SUPPLEMENT',chronology='CAPTURED_AFTER_INITIAL_04A38_IMPLEMENTATION_FROM_PINNED_ENTRY',source_commit=BASE,
   expected_source_phase_hypothesis=original['intended_source_phase'],expected_output='UNKNOWN_NOT_CAPTURED',configurations=['Debug','Release'])
  options=original['parameters']
  if original['family']=='calibration':
   mode=options['weights'].replace('-','_');weights=proposal['weights'][mode]
   weight_value=dict(kind='ExcelMissing') if mode=='missing' else matrix([[v] for v in weights])
   args=[argument('y',matrix([[v] for v in original['y']])),argument('probabilities',matrix([[v] for v in original['probabilities']])),
    argument('bins',dict(kind='Int32',value=options['bins'])),argument('method',cell(options['method'])),argument('weights',weight_value)]
   row['operations']=[dict(role='PUBLIC_EXPORT',target='BESH.PLOT.CALIB_POINTS',arguments=args)]
  elif original['family']=='grouped':
   data=[[1.25,2.0],[2.5,3.25],[3.75,4.5],[5.0,6.25]];labels=[' a ','A','b','']
   if options['row_state']=='nonfinite':
    data[1][0]=dict(kind='Double',bits='7FF8000000000000');data[2][1]=dict(kind='Double',bits='7FF0000000000000')
    data[3]=[dict(kind='ExcelEmpty'),dict(kind='ExcelError',name='ExcelErrorValue')]
   elif options['row_state']=='blank-label':labels[1]=None
   elif options['row_state']=='mixed-case-labels':labels=[' a ','A','B','b']
   header={'none':None,'text':['Feature one','Feature two'],'numeric-text':['1.25','2.5'],'blank':[None,'']}[options['header']]
   if header is not None:data=[header]+data
   grouping=[[label] for label in labels]
   if options['group_header']=='present':grouping=[['Group']]+grouping
   elif options['group_header']=='misaligned':grouping=[['Group'],['Extra header']]+grouping
   source=[argument('data',matrix(data)),argument('groups',matrix(grouping))]
   row['operations']=[dict(role='PUBLIC_EXPORT',target='BESH.ASM.BOX_M',arguments=source),
    dict(role='PRIVATE_PREPARATION_CORROBORATION',owner='BESHStatNG.UdfDataImport',target='TryGetGroupedCompleteMatrix',arguments=source+[argument('groupMatrices',cell(None)),argument('groupNames',cell(None))])]
   row['witness_scope']='Public BOX_M must reach successful import/header/alignment events; with four observations/two variables its subsequent covariance/report guard is not import failure or a claimed successful statistical report.'
  else:
   data=[['G1','S1',1.25],['G1','S2',2.5],['G2','S3',4.0],['G2','S3',5.25]]
   state=options['row_state']
   if state=='blank-group':data[1][0]=None
   elif state=='blank-subgroup':data[1][1]='  '
   elif state=='NaN-response':data[1][2]=dict(kind='Double',bits='7FF8000000000000')
   elif state=='Inf-response':data[1][2]=dict(kind='Double',bits='7FF0000000000000')
   elif state=='mixed-case-labels':data=[[' a ',' s1 ',1.25],['A','S1',2.5],['b','s2',4.0],['B',1.25,5.25]]
   header={'none':None,'text':['Group caption','Subgroup caption','Response caption'],'numeric-text':['Group code','Subgroup code','1.25'],'blank':['',None,'']}[options['header']]
   if header is not None:data=[header]+data
   source=[argument('data',matrix(data))]
   row['operations']=[dict(role='PUBLIC_EXPORT',target='BESH.PAR.ANOVA2_NESTED',arguments=source+[argument('varNames',dict(kind='ExcelMissing')),argument('outputType',dict(kind='ExcelMissing'))]),
    dict(role='PRIVATE_PREPARATION_CORROBORATION',owner='BESHStatNG.UdfDataImport',target='TryGetNestedThreeColumnData',arguments=[argument('input',matrix(data)),argument('data',cell(None)),argument('names',cell(None))])]
   row['witness_scope']='Actual public nested-import branch plus separately recorded helper by-reference data/captions. Later statistical guards are reported separately.'
  records.append(row)
 assert len(records)==432 and len({r['case_id'].casefold() for r in records})==432
 data=''.join(json.dumps(row,ensure_ascii=False,sort_keys=True,separators=(',',':'))+'\n' for row in records).encode('utf-8')
 for name in ['inputs.jsonl','inputs-Debug.jsonl','inputs-Release.jsonl']:
  with (HERE/name).open('xb') as stream:stream.write(data)
 save('pre-capture-materialization-decisions.json',dict(status='BEFORE_FIRST_SUPPLEMENTAL_REFERENCE_CAPTURE',proposal=pin(ROOT/'evidence/04A38/continuation/v1/proposed-missing-case-matrix.json'),
  case_ids_changed=0,case_count_changed=False,decisions=[
   'Resolve hyphenated weight profile IDs to the already proposed underscore-keyed vectors; values unchanged.',
   'Missing calibration weights are explicit ExcelMissing.Value, not a numeric substitute; all array cells/types/bounds are serialized.',
   'Grouped/nested parametric rows are fully materialized with four observations and the approved header/row-state axes. No case family expanded.',
   'Blank headers use Nothing/empty strings; nonfinite grouped state includes unread ExcelEmpty/error cells on the blank-label row, preserving the source early skip.',
   'Nested mixed-case labels include numeric subgroup1.25 to exercise the existing current-culture label conversion; no worksheet row is inferred.',
   'Grouped preparation is recorded on the public BOX_M path and corroborated privately. Four rows/two columns do not establish a successful covariance/report; later guards never count as failed import or successful statistical output.'],
  raw_proposal_preserved=True,reference_execution_yet=False,current_execution_yet=False))
 save('input-seal.json',dict(status='LATE_SUPPLEMENT_INPUTS_SEALED_BEFORE_REFERENCE_AND_CURRENT_EXECUTION',sealed_utc=datetime.now(timezone.utc).isoformat(),source_commit=BASE,
  reference_class='LATE_ARCHIVED_ENTRY_SUPPLEMENT',chronology='CAPTURED_AFTER_INITIAL_04A38_IMPLEMENTATION_FROM_PINNED_ENTRY',not_preedit_frozen=True,
  cases_per_configuration=432,by_family=dict(calibration=288,grouped=96,nested=48),cultures=['','sk-SK'],case_order='Unchanged approved proposal order; no output sorting',
  files=[pin(HERE/name) for name in ['inputs.jsonl','inputs-Debug.jsonl','inputs-Release.jsonl','materialize_inputs.py','pre-capture-materialization-decisions.json','owner-authorization.json','original-source-authenticity.json']],
  source_ordinals='Zero-based array ordinals including any serialized header; model/active ordinals measured only; physical worksheet rows UNKNOWN',
  native='NOT_RUN',scientific='NOT_GOLDEN',expected_outputs='NONE; not generated from current code'))
 print('Sealed432 explicit input cases per configuration:288 calibration/96 grouped/48 nested; no outputs captured')

if __name__=='__main__':main()
