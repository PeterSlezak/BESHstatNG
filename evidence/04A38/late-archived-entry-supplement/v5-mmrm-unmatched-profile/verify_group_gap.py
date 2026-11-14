"""Independent raw original-corpus/source reanalysis of scalar intercept stop."""
import base64,csv,re
from collections import Counter
from pathlib import Path
from verify_inputs import ROOT,PREFIX,need,load,check_pin
from verify_supplement import inspect as supplement
REPORT=PREFIX+'remaining-intercept-group-coverage.json'
def inspect(root=ROOT):
 root=Path(root);supplement(root);report=load(root/REPORT)
 need(report['status']=='NOT_READY_04A38' and report['new_reference_executions']==report['new_case_packets']==report['new_production_changes']==0,'Unapproved reference/production/READY')
 need(report['expected_output']=='UNKNOWN_NOT_CAPTURED' and report['authorization']=='NOT_AUTHORIZED_NOT_EXECUTED','Invented reference/authority')
 need(report['proposed_cases_per_configuration']==2 and report['proposed_cultures']==['Invariant','sk-SK'] and report['configurations_proposed']==['Debug','Release'],'Broadened proposal')
 need(report['proposed_public_entry']=='BESH.REGR.MMRM_LSMEANS' and report['proposed_public_argument_order']==['handle','group','alpha'] and report['proposed_semantic_input']['group_runtime_type']=='System.String' and report['proposed_semantic_input']['group_literal']=='Intercept' and report['proposed_semantic_input']['alpha']=='Nothing','Wrong proposed guard/public path')
 members=load(root/'evidence/04A38/entry-members.json')['members'];member=next(m for m in members if m['name']=='TryGetDesignColumnValues')
 need(report['member_id']==member['symbol_id'] and report['inherited_ownership_row']['ready_disposition']=='NOT_CLOSED','Wrong member or false ownership closure')
 source=(root/'evidence/04A38/preedit/source/Regression/MixedModelUDFs.vb.txt').read_text(encoding='utf-8-sig');lines=source.splitlines()
 guard=report['missing_source_branch'];need(guard['line']==1477 and guard['continuation_line']==1478 and guard['rejection_line']==1479 and lines[1476].strip()==guard['condition'] and lines[1477].strip()==guard['continuation'],'Wrong original source/line map')
 need('"Intercept"' in lines[1476] and '"(Intercept)"' in lines[1477] and 'StringComparison.OrdinalIgnoreCase' in lines[1477] and 'the intercept column cannot be used as a grouping factor.' in lines[1478],'Missing intended intercept rejection')
 observation=load(root/(PREFIX+'group-selection-coverage-observation.json'));check_pin(root,report['observations'])
 corpus=[('original-v1','frozen-v2'),('public-options-v1','attempts/public-options-v1'),('public-primitives-v1','attempts/public-primitives-v1'),('model-inputs-v2','attempts/model-inputs-v2'),('math-edges-v1','attempts/math-edges-v1')]
 results={};witnesses=[]
 for config in ['Debug','Release']:
  counts=Counter()
  for name,relative in corpus:
   base=root/'evidence/04A38/preedit'/relative;cases=[load_line for load_line in map(__import__('json').loads,(base/'cases.jsonl').read_bytes().splitlines())]
   trace=root/'evidence/04A38/preedit/attempts/capture-v10'/('capture-'+config+'-trace-v1.tsv.reachability.jsonl') if name=='original-v1' else base/('capture-'+config+'-trace-v1.tsv.reachability.jsonl')
   hits={r['case_id']:r['reached'] for r in map(__import__('json').loads,trace.read_bytes().splitlines())}
   output=base/('original-'+config+'.tsv' if name=='original-v1' else 'capture-'+config+'-original-v1.tsv')
   with output.open(encoding='utf-8',newline='') as stream:rows={r['case_id']:r for r in csv.DictReader(stream,delimiter='\t')}
   for c in cases:
    if c.get('target')=='BESH.REGR.MMRM_LSMEANS':counts['public_lsmeans_cases']+=1
    if member['symbol_id'] not in hits[c['case_id']]:continue
    need(c.get('target') in ['BESH.REGR.MMRM_LSMEANS','BESH.REGR.MMRM_CONTRASTS'],'Omitted indirect/private helper witness')
    r=rows[c['case_id']];counts['helper_entry_cases']+=1
    if r['outcome']=='RETURN_ARRAY':counts['report_after_helper']+=1
    decoded=base64.b64decode(r['result'][1:]).decode() if r['outcome']=='RETURN_TEXT' else None
    if decoded and 'the intercept column cannot be used as a grouping factor.' in decoded:counts['intercept_guard_outputs']+=1
    witnesses.append(dict(configuration=config,case_id=c['case_id'],culture=c['culture'],target=c['target'],original_input=r['byref_arguments'],outcome=r['outcome'],decoded_return_text=decoded))
  counts['intercept_guard_outputs']+=0;results[config]=dict(counts)
 need(results==report['configurations']==observation['configurations'],'Producer coverage differs from raw original corpus')
 need(all(c==dict(public_lsmeans_cases=66,helper_entry_cases=34,report_after_helper=16,intercept_guard_outputs=0) for c in results.values()),'Incorrect guard/reference coverage')
 need(witnesses==observation['witnesses'] and len(witnesses)==68,'Lost/modified witness or source-row/input context')
 for r in observation['inputs']:check_pin(root,r)
 for config in ['Debug','Release']:
  for version in ['v1','v2-gee-profile','v3-mmrm-headered-profile','v4-mmrm-duplicate-guards','v5-mmrm-unmatched-profile']:
   packet=root/'evidence/04A38/late-archived-entry-supplement'/version
   cases=list(map(__import__('json').loads,(packet/('inputs-'+config+'.jsonl')).read_bytes().splitlines()))
   if version=='v5-mmrm-unmatched-profile':need(all(c['operations'][1]['arguments'][1]['value']['kind']=='Array' for c in cases),'Borrowed scalar case from Approval E')
   else:need(all(o['target'] not in ['BESH.REGR.MMRM_LSMEANS','BESH.REGR.MMRM_CONTRASTS'] for c in cases for o in c['operations']),'Unreviewed prior late scalar witness')
 for name in ['MMRM_FIT','MMRM_LSMESTIMATE']:
  block=re.search(r'(?m)^\s*Public Function '+name+r'\b[\s\S]*?^\s*End Function',source)[0];need('TryGetDesignColumnValues' not in block,'Omitted indirect named-group call')
 return dict(status='VALID_NOT_READY_SCALAR_INTERCEPT_REFERENCE_GAP',member_id=member['symbol_id'],configurations=results,new_references=0,new_production_changes=0)
if __name__=='__main__':
 import json
 print(json.dumps(inspect()))
