"""Independent read-only original-supplement checker; standard library only.

No producer/generator or Core/Common code is imported. It cannot approve full
04A38 readiness; verifies this bounded original-source coverage gate only.
"""
import csv,hashlib,json,re
from collections import Counter,defaultdict
from pathlib import Path

ROOT=Path(__file__).resolve().parents[4];HERE=Path(__file__).resolve().parent
BASE='ba1636f5356ac761509c2cfa16fc66b92430935d'
class InvalidSupplement(ValueError):pass
def require(value,message):
 if not value:raise InvalidSupplement(message)
def digest(data):return hashlib.sha256(data).hexdigest()
def read(path):return json.loads(path.read_bytes())
def validate_input(records):
 require(len(records)==432 and len({r['case_id'].casefold() for r in records})==432,'Missing/duplicate/casefold case')
 require(Counter(r['family'] for r in records)==Counter(calibration=288,grouped=96,nested=48),'Family matrix changed')
 for row in records:
  require(row['culture'] in {'','sk-SK'} and row['source_commit']==BASE,'Wrong culture/pinned commit')
  require(row['expected_output']=='UNKNOWN_NOT_CAPTURED' and row['chronology']=='CAPTURED_AFTER_INITIAL_04A38_IMPLEMENTATION_FROM_PINNED_ENTRY','False expectation/chronology')
  for op in row['operations']:
   for argument in op['arguments']:validate_value(argument['value'])
def validate_value(value):
 kind=value['kind'];require(kind in {'Array','Double','Int32','String','Nothing','ExcelMissing','ExcelEmpty','ExcelError'},'Unknown cell kind')
 if kind=='Double':require(bool(re.fullmatch('[0-9A-F]{16}',value['bits'])),'Malformed IEEE input')
 if kind=='Array':
  require(value['element_type']=='System.Object' and value['rank']==2,'Wrong array rank/type')
  require(len(value['lengths'])==len(value['lower_bounds'])==len(value['upper_bounds'])==2,'Missing bounds')
  require(all(value['lower_bounds'][i]+value['lengths'][i]-1==value['upper_bounds'][i] for i in range(2)),'Wrong array upper/lower bounds')
  require(value['lengths'][0]*value['lengths'][1]==len(value['values']),'Wrong cell cardinality')
  require(value['physical_worksheet_rows']=='UNKNOWN','Invented worksheet rows')
  for item in value['values']:validate_value(item)
def load_rows(path):
 with path.open(encoding='utf-8',newline='') as f:return list(csv.DictReader(f,delimiter='\t'))
def inspect():
 approval=read(HERE/'owner-authorization.json')
 require(approval['owner_quote']=='Yes, I approve' and approval['owner_decision_date']=='2026-11-08' and approval['source_commit']==BASE,'Missing/wrong owner authority')
 auth=read(HERE/'original-source-authenticity.json');require(auth['source_commit']==BASE and len(auth['files'])==4695,'Wrong/unavailable original source tree')
 seal=read(HERE/'input-seal.json');require(seal['not_preedit_frozen'] and seal['cases_per_configuration']==432,'Mislabelled late capture')
 for item in seal['files']:
  data=(ROOT/item['path']).read_bytes();require(len(data)==item['bytes'] and digest(data)==item['sha256'],'Unfrozen/altered input or source authority')
 raw=(HERE/'inputs.jsonl').read_bytes();cases=[json.loads(line) for line in raw.decode().splitlines()];validate_input(cases)
 proposal=read(ROOT/'evidence/04A38/continuation/v1/proposed-missing-case-matrix.json')
 require([r['case_id'] for r in cases]==[r['case_id'] for r in proposal['cases']],'Case IDs reordered/replaced')
 results={};witnesses=[]
 for config in ['Debug','Release']:
  require((HERE/('inputs-'+config+'.jsonl')).read_bytes()==raw,'Configuration inputs differ')
  folders={mode:HERE/'original-results'/config/(mode+'-v2') for mode in ['plain','repeat','trace']}
  outputs=[(folder/'results.tsv').read_bytes() for folder in folders.values()]
  require(outputs[0]==outputs[1]==outputs[2],'Original repeat/instrumentation changes output or exception/byref phase')
  rows=load_rows(folders['plain']/'results.tsv');trace=read(folders['trace']/'results.tsv.branches.json')
  require(len(rows)==len(trace)==432 and [r['case_id'] for r in rows]==[r['case_id'] for r in cases]==[r['case_id'] for r in trace],'Missing/duplicated/reordered execution cases')
  for mode,folder in folders.items():
   execution=read(folder/'execution.json');require(execution['cases']==432 and execution['native_attempts']==0 and execution['role']=='original','Native/role/case mismatch')
   build=read(HERE/('compile-original-'+config+('-trace' if mode=='trace' else '-plain')+'.json'))
   allowed={a['name']:a['sha256'] for a in build['dependencies']}
   require(all(a['sha256']==allowed[a['name']+'.dll'] for a in execution['assemblies']),'Loaded assembly is not compiled original')
  counts=defaultdict(Counter)
  for case,row,events in zip(cases,rows,trace):
   key=case['family']+'/'+(case['culture'] or 'Invariant');operations=json.loads(row['operations']);public=operations[0];public_events=events['operations'][0]['events'];counts[key]['cases']+=1
   require(row['input_sha256']==digest((json.dumps(case,ensure_ascii=False,sort_keys=True,separators=(',',':'))+'\n').encode()),'Unmatched input hash')
   require(all(op['kind']!='EXCEPTION' for op in operations),'Uncaught harness/source exception; investigate before sealing')
   if public['kind']=='RETURN_ARRAY':counts[key]['public_return_array']+=1
   else:counts[key]['public_error_cell']+=1;require(public['kind']=='RETURN_ERROR_CELL','Unexpected public kind')
   if case['family']=='calibration':
    report=[v for v in public_events if v.startswith('calibration.report_enter=')]
    nan=[v for v in public_events if v in ['calibration.lower_is_nan=True','calibration.upper_is_nan=True']]
    if report:
     require(public['kind']=='RETURN_ARRAY','Guard incorrectly counted as calibration report')
     counts[key]['report_reached']+=1
     if 'calibration.method=equalwidth' in public_events:counts[key]['equalwidth_report_reached']+=1
     if case['parameters']['weights']!='missing':counts[key]['supplied_weight_report_reached']+=1
     if nan:
      counts[key]['nonfinite_CI_report_reached']+=1
      # Source's mixed Enum/Double ternary widens NA42 to binary64. Record the
      # actual type/bits; no assumed ExcelError token or behavioral correction.
      require('D4045000000000000' in public['result'],'Observed nonfinite-arm cell representation missing')
      counts[key]['observed_NA_arm_binary64_42_reports']+=1
    else:counts[key]['guard_before_report']+=1
   else:
    prefix='grouped.prepared_groups=' if case['family']=='grouped' else 'nested.prepared_rows='
    prepared=any(v.startswith(prefix) for v in public_events)
    helper=operations[1];require(helper['kind']=='RETURN_SCALAR' and helper['result'] in {'B1','B0'},'Missing direct preparation corroboration')
    require(prepared==(helper['result']=='B1'),'Public dispatch and private preparation disagree')
    if prepared:
     counts[key]['preparation_success_public_path']+=1
     if public['kind']=='RETURN_ERROR_CELL':counts[key]['later_public_error_after_preparation']+=1
     header=(case['family']+'.header=True') in public_events
     if header:counts[key]['header_preparation_success_public_path']+=1
    else:counts[key]['guard_before_preparation']+=1
   witnesses.append(dict(configuration=config,culture=case['culture'],family=case['family'],case_id=case['case_id'],parameters=case['parameters'],public_kind=public['kind'],source_events=public_events))
  for culture in ['Invariant','sk-SK']:
   cal=counts['calibration/'+culture];group=counts['grouped/'+culture];nested=counts['nested/'+culture]
   require(cal['cases']==144 and cal['report_reached']>0 and cal['equalwidth_report_reached']>0 and cal['supplied_weight_report_reached']>0 and cal['nonfinite_CI_report_reached']>0 and cal['guard_before_report']>0,'Missing required calibration positive/negative branch')
   require(group['cases']==48 and group['header_preparation_success_public_path']>0 and group['guard_before_preparation']>0,'Missing grouped header/alignment positive/guard')
   require(nested['cases']==24 and nested['header_preparation_success_public_path']>0,'Missing nested response header success')
  results[config]=dict(cases=432,output_sha256=digest(outputs[0]),bytes=len(outputs[0]),repeat_exact=True,trace_output_exact=True,by_family_culture={k:dict(v) for k,v in counts.items()})
 return dict(status='ORIGINAL_LATE_SUPPLEMENT_COVERAGE_PASS_NOT_BATCH_READY',source_commit=BASE,configurations=results,witnesses=witnesses,
  actual_nonfinite_CI_error_arm='System.Double42.0 (IEEE4045000000000000), not an ExcelError cell, in original mixed Enum/Double If expression. Preserve this observed legacy behavior; scientific/native correction unapproved.',
  qualification='Grouped public BOX_M has later statistical guards under the approved four-row matrix. Successful import/header/alignment is proven on its public path and corroborated by actual helper byrefs; no successful BoxM statistical report is claimed.',
  chronology='CAPTURED_AFTER_INITIAL_04A38_IMPLEMENTATION_FROM_PINNED_ENTRY',reference_class='LATE_ARCHIVED_ENTRY_SUPPLEMENT',native='NOT_RUN',scientific='NOT_GOLDEN')

if __name__=='__main__':
 try:
  result=inspect();out=HERE/'original-coverage.json'
  with out.open('x',encoding='utf-8',newline='\n') as f:json.dump(result,f,indent=2);f.write('\n')
  print(json.dumps({k:v for k,v in result.items() if k not in ['witnesses']}))
 except (InvalidSupplement,OSError,ValueError,KeyError) as error:print(json.dumps(dict(status='NOT_READY_04A38',reason=str(error))));raise SystemExit(1)
