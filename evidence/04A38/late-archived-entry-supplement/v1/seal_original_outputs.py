"""Seal original outputs before any execution of current supplemental cases."""
from datetime import datetime,timezone
import json
from prepare_original import ROOT,HERE,BASE,pin,save

def main():
 coverage=json.loads((HERE/'original-coverage.json').read_bytes())
 assert coverage['status']=='ORIGINAL_LATE_SUPPLEMENT_COVERAGE_PASS_NOT_BATCH_READY'
 assert not (HERE/'current-results').exists()
 files=[]
 for config in ['Debug','Release']:
  for mode in ['plain','repeat','trace']:
   folder=HERE/'original-results'/config/(mode+'-v2')
   files.extend(pin(p) for p in sorted(folder.iterdir()) if p.is_file())
   files.append(pin(HERE/('original-execution-'+config+'-'+mode+'-v2.json')))
  files.extend(pin(HERE/name) for name in ['build-original-'+config+'.json','compile-original-'+config+'-plain.json','compile-original-'+config+'-trace.json'])
 files.extend(pin(HERE/name) for name in ['owner-authorization.json','original-source-authenticity.json','input-seal.json','original-coverage.json','LateCapture38.cs',
  'compile_original_harness.py','run_original_capture.py','verify_original_coverage.py','seal_original_outputs.py'])
 save('original-output-seal.json',dict(status='ORIGINAL_LATE_OUTPUTS_SEALED_BEFORE_FIRST_CURRENT_SUPPLEMENT_EXECUTION',source_commit=BASE,
  owner_quote='Yes, I approve',owner_date='2026-11-08',sealed_utc=datetime.now(timezone.utc).isoformat(),files=files,
  case_count_per_configuration=432,first_valid_original_outputs={config:pin(HERE/'original-results'/config/'plain-v2/results.tsv') for config in ['Debug','Release']},
  repeat_and_instrumentation_byte_exact=True,positive_coverage='original-coverage.json; successful import on public paths distinguished from later statistical errors',
  current_supplement_executed_at_seal=False,current_results_directory_absent=True,
  input_seal=pin(HERE/'input-seal.json'),reference_class='LATE_ARCHIVED_ENTRY_SUPPLEMENT',chronology='CAPTURED_AFTER_INITIAL_04A38_IMPLEMENTATION_FROM_PINNED_ENTRY',
  is_pre_edit_frozen=False,native='NOT_RUN',scientific='NOT_GOLDEN',scope='Bounded432 cases only; original28966 pre-production corpus unchanged and separate'))
 print('Sealed original432 outputs/configuration before any current supplemental run; current outputs absent')

if __name__=='__main__':main()
