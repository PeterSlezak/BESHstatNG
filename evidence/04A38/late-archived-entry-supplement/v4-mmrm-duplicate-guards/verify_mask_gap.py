"""Independent raw-corpus/source reanalysis of the empty-selection stop gate."""
import base64,csv,json,re
from collections import Counter
from pathlib import Path
from verify_inputs import ROOT,PREFIX,need,load,check_pin
from verify_supplement import inspect as supplement
REPORT=PREFIX+'remaining-profile-mask-coverage.json'

def inspect(root=ROOT):
 root=Path(root);supplement(root);report=load(root/REPORT)
 need(report['status']=='NOT_READY_04A38' and report['new_reference_executions']==report['new_case_packets']==report['new_production_changes']==0,'Unauthorized reference/materialization/production/READY')
 need(report['expected_output']=='UNKNOWN_NOT_CAPTURED','Invented original unmatched-profile result')
 entry=load(root/'evidence/04A38/entry-members.json')['members'];member=next(m for m in entry if m['name']=='TryBuildMmrmAtProfileRowMask')
 need(report['member_id']==member['symbol_id'],'Wrong remaining source member')
 source=(root/'evidence/04A38/preedit/source/Regression/MixedModelUDFs.vb.txt').read_text(encoding='utf-8-sig')
 guard=report['missing_source_branch'];need(guard['line']==1426 and guard['condition']=='If matchedCount <= 0 Then' and source.splitlines()[1425].strip()==guard['condition'],'Wrong original empty-mask source line/condition')
 need('no observed design rows matched the supplied LS-means profile.' in source.splitlines()[1426],'Lost source error arm')
 corpus=[('original-v1','frozen-v2'),('public-options-v1','attempts/public-options-v1'),('public-primitives-v1','attempts/public-primitives-v1'),('model-inputs-v2','attempts/model-inputs-v2'),('math-edges-v1','attempts/math-edges-v1')]
 observations={};witnesses={}
 for config in ['Debug','Release']:
  counts=Counter()
  for name,relative in corpus:
   base=root/'evidence/04A38/preedit'/relative;cases=[json.loads(line) for line in (base/'cases.jsonl').read_bytes().splitlines()]
   trace=(root/'evidence/04A38/preedit/attempts/capture-v10'/('capture-'+config+'-trace-v1.tsv.reachability.jsonl')) if name=='original-v1' else base/('capture-'+config+'-trace-v1.tsv.reachability.jsonl')
   hits={r['case_id']:r['reached'] for r in map(json.loads,trace.read_bytes().splitlines())}
   output=base/('original-'+config+'.tsv' if name=='original-v1' else 'capture-'+config+'-original-v1.tsv')
   with output.open(encoding='utf-8',newline='') as stream:rows={r['case_id']:r for r in csv.DictReader(stream,delimiter='\t')}
   for case in cases:
    if case.get('target')=='BESH.REGR.MMRM_LSMEANS':counts['original_public_lsmeans_cases']+=1
    if member['symbol_id'] not in hits[case['case_id']]:continue
    need(case['target']=='BESH.REGR.MMRM_LSMEANS','Omitted indirect/direct mask witness')
    r=rows[case['case_id']];counts['mask_entry_cases']+=1
    if r['outcome']=='RETURN_ARRAY':counts['returned_report_after_mask']+=1
    text=base64.b64decode(r['result'][1:]).decode() if r['outcome']=='RETURN_TEXT' else None
    if text and 'no observed design rows matched the supplied LS-means profile.' in text:counts['no_matching_rows_guard_outputs']+=1
    witnesses[(config,case['case_id'])]=(r['byref_arguments'],r['outcome'],text)
  counts['no_matching_rows_guard_outputs']+=0
  for packet in ['v3-mmrm-headered-profile','v4-mmrm-duplicate-guards']:
   p=root/'evidence/04A38/late-archived-entry-supplement'/packet/('inputs-'+config+'.jsonl')
   cases=[json.loads(line) for line in p.read_bytes().splitlines()]
   need(all(c['operations'][1]['target']=='BESH.REGR.MMRM_LSMESTIMATE' for c in cases),'Misclassified later MMRM public family')
  estimate=re.search(r'(?m)^\s*Public Function MMRM_LSMESTIMATE\b[\s\S]*?^\s*End Function',source)[0]
  need('TryBuildMmrmAtProfileRowMask' not in estimate,'Unreviewed indirect mask call in estimate source')
  counts.update(late_headered_lsmeans_mask_cases=0,late_duplicate_lsmeans_mask_cases=0)
  observations[config]=dict(counts)
 need(observations==report['configurations'],'Published mask coverage differs from independent raw analysis')
 for counts in observations.values():need(counts==dict(original_public_lsmeans_cases=66,mask_entry_cases=4,returned_report_after_mask=4,no_matching_rows_guard_outputs=0,late_headered_lsmeans_mask_cases=0,late_duplicate_lsmeans_mask_cases=0),'Wrong original mask coverage counts')
 need(len(report['witnesses'])==len(witnesses)==8,'Omitted/extra mask witness')
 for r in report['witnesses']:need((r['original_input'],r['outcome'],r['decoded_return_text'])==witnesses[(r['configuration'],r['case_id'])],'False success/guard classification')
 for row in report['inputs']:check_pin(root,row)
 need(report['inherited_ownership_row']['ready_disposition']=='NOT_CLOSED','Unsupported ownership closure')
 return dict(status='VALID_NOT_READY_EMPTY_MMRM_MASK_REFERENCE_GAP',member_id=member['symbol_id'],configurations=observations,new_references=0,new_production_changes=0)
if __name__=='__main__':print(json.dumps(inspect()))
