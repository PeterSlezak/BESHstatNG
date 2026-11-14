"""Read-only resumed body/reference review; no model or reference execution."""
import base64,csv,re
from collections import Counter
from common import *
def main():
 assert json.loads((HERE/'MMRM_UNMATCHED_PROFILE_GATE_PASS.json').read_bytes())['status']=='MMRM_UNMATCHED_PROFILE_GATE_PASS'
 members=json.loads((ROOT/'evidence/04A38/entry-members.json').read_bytes())['members']
 member=next(m for m in members if m['name']=='TryGetDesignColumnValues')
 source=ROOT/'evidence/04A38/preedit/source/Regression/MixedModelUDFs.vb.txt'
 text=source.read_text(encoding='utf-8-sig');block=re.search(r'(?m)^\s*Private Function TryGetDesignColumnValues\b[\s\S]*?^\s*End Function',text)[0]
 print(member['symbol_id'])
 print([(i+1,s.strip()) for i,s in enumerate(text.splitlines()) if 'the intercept column cannot be used as a grouping factor.' in s])
 message='the intercept column cannot be used as a grouping factor.'
 corpora=[('original-v1','frozen-v2'),('public-options-v1','attempts/public-options-v1'),('public-primitives-v1','attempts/public-primitives-v1'),('model-inputs-v2','attempts/model-inputs-v2'),('math-edges-v1','attempts/math-edges-v1')]
 configurations={};witnesses=[];pins=[]
 for config in ['Debug','Release']:
  counts=Counter()
  for corpus,relative in corpora:
   base=ROOT/'evidence/04A38/preedit'/relative;cases=[json.loads(line) for line in (base/'cases.jsonl').read_bytes().splitlines()]
   trace=ROOT/'evidence/04A38/preedit/attempts/capture-v10'/('capture-'+config+'-trace-v1.tsv.reachability.jsonl') if corpus=='original-v1' else base/('capture-'+config+'-trace-v1.tsv.reachability.jsonl')
   reached={r['case_id']:r['reached'] for r in map(json.loads,trace.read_bytes().splitlines())}
   output=base/('original-'+config+'.tsv' if corpus=='original-v1' else 'capture-'+config+'-original-v1.tsv')
   with output.open(encoding='utf-8',newline='') as stream:rows={r['case_id']:r for r in csv.DictReader(stream,delimiter='\t')}
   pins.extend([pin(base/'cases.jsonl'),pin(trace),pin(output)])
   for c in cases:
    if c.get('target')=='BESH.REGR.MMRM_LSMEANS':counts['public_lsmeans_cases']+=1
    if member['symbol_id'] not in reached[c['case_id']]:continue
    r=rows[c['case_id']];counts['helper_entry_cases']+=1
    decoded=base64.b64decode(r['result'][1:]).decode() if r['outcome']=='RETURN_TEXT' else None
    if decoded and message in decoded:counts['intercept_guard_outputs']+=1
    if r['outcome']=='RETURN_ARRAY':counts['report_after_helper']+=1
    witnesses.append(dict(configuration=config,case_id=c['case_id'],culture=c['culture'],target=c.get('target'),original_input=r['byref_arguments'],outcome=r['outcome'],decoded_return_text=decoded))
  counts['intercept_guard_outputs']+=0;configurations[config]=dict(counts)
 report=dict(status='POST_GATE_SOURCE_COVERAGE_OBSERVATION_NOT_A_NEW_REFERENCE',member_id=member['symbol_id'],signature=member['signature'],source_snapshot=pin(source),source_body=block,configurations=configurations,witnesses=witnesses,inputs=pins,new_original_reference_executions=0,new_case_packets=0,new_production_changes=0)
 save('group-selection-coverage-observation.json',report)
 print(json.dumps(dict(configurations=configurations,witnesses=witnesses),ensure_ascii=False))
if __name__=='__main__':main()
