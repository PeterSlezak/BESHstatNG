"""Independent bounded late-supplement verifier; never a batch READY decision."""
import csv,hashlib,json
from datetime import datetime
from pathlib import Path

ROOT=Path(__file__).resolve().parents[4];PREFIX='evidence/04A38/late-archived-entry-supplement/v1/'
SEAL='031d54ad25ad01f2c85ece31b136aaa2669fb4787f45a346f0a0321132845459'
class InvalidSupplement(ValueError):pass
def need(value,message):
 if not value:raise InvalidSupplement(message)
def digest(data):return hashlib.sha256(data).hexdigest()
def inspect(root=ROOT):
 here=root/PREFIX
 def load(path):return json.loads(path.read_bytes())
 seal_data=(here/'original-output-seal.json').read_bytes();need(digest(seal_data)==SEAL,'Altered original seal/chronology')
 seal=load(here/'original-output-seal.json')
 need(not seal['is_pre_edit_frozen'] and not seal['current_supplement_executed_at_seal'],'Late reference mislabeled or after current execution')
 for row in seal['files']:
  data=(root/row['path']).read_bytes();need(len(data)==row['bytes'] and digest(data)==row['sha256'],'Altered original source/output/control')
 input_seal=load(here/'input-seal.json')
 for row in input_seal['files']:
  data=(root/row['path']).read_bytes();need(len(data)==row['bytes'] and digest(data)==row['sha256'],'Altered materialized input/culture/bounds/value')
 approval=load(here/'owner-authorization.json');need(approval['owner_quote']=='Yes, I approve' and approval['owner_decision_date']=='2026-11-08','Missing owner decision')
 auth=load(here/'original-source-authenticity.json');need(auth['source_commit']=='ba1636f5356ac761509c2cfa16fc66b92430935d','Wrong original commit')
 raw=(here/'inputs.jsonl').read_bytes();inputs=[json.loads(line) for line in raw.decode().splitlines()]
 need(len(inputs)==432 and len({r['case_id'].casefold() for r in inputs})==432,'Missing/duplicate cases')
 results={}
 for config in ['Debug','Release']:
  need((here/('inputs-'+config+'.jsonl')).read_bytes()==raw,'Mismatched configuration inputs')
  reference=here/'original-results'/config/'plain-v2/results.tsv';current=here/'current-results'/config/'results.tsv'
  need(reference.read_bytes()==current.read_bytes(),'Original/current result mismatch; no tolerance permitted')
  trace=here/'original-results'/config/'trace-v2/results.tsv.branches.json';current_trace=here/'current-results'/config/'results.tsv.branches.json'
  need(trace.read_bytes()==current_trace.read_bytes(),'Changed/omitted actual branch witnesses')
  with current.open(encoding='utf-8',newline='') as f:rows=list(csv.DictReader(f,delimiter='\t'))
  need([r['case_id'] for r in rows]==[r['case_id'] for r in inputs],'Missing/reordered/duplicate current row')
  current_receipt=load(here/('current-execution-'+config+'.json'))
  need(current_receipt['original_output_seal']['sha256']==SEAL,'Current lacks sealed original dependency')
  execution=load(here/'current-results'/config/'execution.json')
  need(execution['role']=='current' and execution['native_attempts']==0,'Wrong runtime/native boundary')
  need(datetime.fromisoformat(execution['started_utc'].replace('Z','+00:00'))>datetime.fromisoformat(seal['sealed_utc']),'Current execution preceded original seal')
  coverage=load(here/'original-coverage.json')['configurations'][config]
  need(coverage['repeat_exact'] and coverage['trace_output_exact'],'Instrumentation/repeat mismatch')
  for culture in ['Invariant','sk-SK']:
   counts=coverage['by_family_culture'];cal=counts['calibration/'+culture];group=counts['grouped/'+culture];nested=counts['nested/'+culture]
   need(cal['report_reached']==90 and cal['equalwidth_report_reached']==60 and cal['nonfinite_CI_report_reached']==36 and cal['guard_before_report']==54,'Missing/misclassified calibration witnesses')
   need(group['preparation_success_public_path']==32 and group['header_preparation_success_public_path']==24 and group['guard_before_preparation']==16,'Grouped guards mislabeled success')
   need(nested['preparation_success_public_path']==24 and nested['header_preparation_success_public_path']==18,'Nested header not reached')
  results[config]=dict(cases=432,bytes=current.stat().st_size,sha256=digest(current.read_bytes()),exact=True,actual_branches_exact=True)
 return dict(status='LATE_SUPPLEMENT_EXACT_AND_COVERAGE_PASS_NOT_BATCH_READY',configurations=results,original_rows_separate=28966,
  reference_class='LATE_ARCHIVED_ENTRY_SUPPLEMENT',chronology='CAPTURED_AFTER_INITIAL_04A38_IMPLEMENTATION_FROM_PINNED_ENTRY',native='NOT_RUN',scientific='NOT_GOLDEN',
  remaining='All-statement migration ownership, remaining production extraction and full independent acceptance audit are not certified by this bounded verifier')

if __name__=='__main__':
 try:print(json.dumps(inspect()))
 except (InvalidSupplement,OSError,ValueError,KeyError,TypeError) as error:print(json.dumps(dict(status='NOT_READY_04A38',reason=str(error))));raise SystemExit(1)
