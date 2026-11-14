"""Independent original-corpus/source review of the mandatory duplicate-guard stop."""
import base64,csv,json,re
from collections import Counter
from pathlib import Path
from verify_inputs import ROOT,PREFIX,need,load,check_pin
from verify_supplement import inspect as supplement
REPORT=PREFIX+'remaining-duplicate-guard-coverage-v2.json'

def array(token):
 if not re.match(r'^System\.[\w.]+\[[\d,:-]+\]\{',token):return None
 start=token.index('{');header=token[:start];body=token[start+1:-1];dims=[tuple(map(int,s.split(':'))) for s in header[header.index('[')+1:-1].split(',')]
 values=[];depth=0;last=0
 for i,ch in enumerate(body):
  if ch=='{':depth+=1
  elif ch=='}':depth-=1
  elif ch=='|' and depth==0:values.append(body[last:i]);last=i+1
 if body:values.append(body[last:])
 from math import prod
 need(depth==0 and len(values)==prod(n for _,n in dims),'Invalid canonical array')
 return dims,values

def inspect(root=ROOT):
 root=Path(root);report=load(root/REPORT);supplement(root)
 need(report['status']=='NOT_READY_04A38' and report['new_invalid_profile_materializations']==report['new_invalid_profile_executions']==report['new_production_changes']==0,'Unauthorized capture/production/READY')
 need(report['expected_results']=='UNKNOWN_NOT_CAPTURED','Fabricated original guard result')
 entry=load(root/'evidence/04A38/entry-members.json')['members'];member=next(m for m in entry if m['name']=='AddMmrmLsmEstimateAtValue')
 need(report['member_id']==member['symbol_id'],'Wrong missing member identity')
 source=(root/'evidence/04A38/preedit/source/UDFdataImport/UdfDataImportMixedModel.vb.txt').read_text(encoding='utf-8-sig').splitlines()
 for guard in report['missing_source_branches']:need(source[guard['line']-1].strip()==guard['condition'],'Incorrect archived guard source/line')
 need([(g['line'],g['condition']) for g in report['missing_source_branches']]==[(726,'If atProfile.VisitSpecified Then'),(744,'If existing.ColumnIndex = idx Then')],'Missing/weakened duplicate source branch')
 corpora=[('original-v1','frozen-v2'),('public-options-v1','attempts/public-options-v1'),('public-primitives-v1','attempts/public-primitives-v1'),('model-inputs-v2','attempts/model-inputs-v2'),('math-edges-v1','attempts/math-edges-v1')]
 observations={};witnesses={}
 for config in ['Debug','Release']:
  counts=Counter()
  for corpus,relative in corpora:
   base=root/'evidence/04A38/preedit'/relative
   cases=[json.loads(line) for line in (base/'cases.jsonl').read_bytes().splitlines()]
   trace=(root/'evidence/04A38/preedit/attempts/capture-v10'/('capture-'+config+'-trace-v1.tsv.reachability.jsonl')) if corpus=='original-v1' else base/('capture-'+config+'-trace-v1.tsv.reachability.jsonl')
   hits={r['case_id']:r['reached'] for r in map(json.loads,trace.read_bytes().splitlines())}
   output=base/('original-'+config+'.tsv' if corpus=='original-v1' else 'capture-'+config+'-original-v1.tsv')
   with output.open(encoding='utf-8',newline='') as stream:rows={r['case_id']:r for r in csv.DictReader(stream,delimiter='\t')}
   for case in cases:
    key=case['case_id'];target=case.get('target');entered=member['symbol_id'] in hits[key]
    if target not in ['BESH.REGR.MMRM_LSMEANS','BESH.REGR.MMRM_LSMESTIMATE']:
     need(not entered,'Unreviewed indirect guard witness');continue
    counts['original_public_cases']+=1
    _,args=array(rows[key]['byref_arguments']);a=array(args[1 if target.endswith('_LSMEANS') else 3])
    if a is None:continue
    dims,values=a;counts['original_array_profiles']+=1
    if entered:counts['original_helper_entry_cases']+=1
    names=[];kind=None
    if dims==[(0,2),(0,2)] and values[0]=='SdHJlYXRtZW50X2FjdGl2ZQ==':
     if values[1].startswith('D'):names=[base64.b64decode(values[0][1:]).decode(),base64.b64decode(values[2][1:]).decode()];kind='HEADERLESS_VALID'
     elif values[1]=='SdmlzaXQ=':names=['treatment_active','visit'];kind='WIDE_VALID'
    if kind is None:
     need(all(v.startswith('D') for v in values),'Unreviewed named profile shape');kind='EARLY_NUMERIC_HEADER_ERROR'
    normalized=['visit' if ''.join(ch for ch in s.lower() if ch.isalnum()) in ['visit','time'] else ''.join(ch for ch in s.lower() if ch.isalnum()) for s in names]
    duplicate=len(normalized)!=len(set(normalized))
    if duplicate:counts['original_duplicate_selection_candidates']+=1
    r=rows[key];message=base64.b64decode(r['result'][1:]).decode() if r['outcome']=='RETURN_TEXT' else ''
    if ('at specifies visit/time more than once.' in message or ('at specifies design column' in message and 'more than once.' in message)):counts['original_duplicate_guard_outputs']+=1
    witnesses[(config,key)]=(kind,names,duplicate,entered,r['outcome'])
  traces=load(root/(PREFIX+'original-results/'+config+'/trace/results.tsv.branches.json'))
  need(len(traces)==2,'Approved headered cases missing/expanded')
  for trace in traces:
   events=trace['operations'][1]['events'];need([e for e in events if e.startswith('helper.entry=')]==['helper.entry=treatment_active','helper.entry=visit'] and 'report.returned=True' in events,'Headered positive case cannot be duplicate guard witness')
  counts.update(approved_headered_cases=2,approved_headered_helper_entry_cases=2,approved_headered_reports=2)
  for k in ['original_duplicate_selection_candidates','original_duplicate_guard_outputs','approved_headered_duplicate_guard_witnesses']:counts[k]+=0
  observations[config]=dict(counts)
 need(observations==report['configurations'],'Published guard coverage differs from independent corpus analysis')
 for row in observations.values():need(row==dict(original_public_cases=154,original_array_profiles=12,original_helper_entry_cases=10,approved_headered_cases=2,approved_headered_helper_entry_cases=2,approved_headered_reports=2,original_duplicate_selection_candidates=0,original_duplicate_guard_outputs=0,approved_headered_duplicate_guard_witnesses=0),'Wrong source coverage count')
 need(len(report['witnesses'])==len(witnesses)==24,'Omitted/extra original profile witnesses')
 for r in report['witnesses']:need((r['kind'],r['selected_names'],r['duplicate_candidate'],r['helper_entered'],r['outcome'])==witnesses[(r['configuration'],r['case_id'])],'Misclassified positive/helper/guard case')
 for row in report['inputs']:check_pin(root,row)
 with (root/report['ownership_ledger']['path']).open(encoding='utf-8-sig',newline='') as stream:ledger=next(r for r in csv.DictReader(stream) if r['original_member_id']==report['member_id'])
 need(ledger==report['inherited_ownership_row'] and ledger['ready_disposition']=='NOT_CLOSED','Unsupported member closure/HOST_ONLY')
 return dict(status='VALID_NOT_READY_MMRM_DUPLICATE_REFERENCE_GAP',member_id=report['member_id'],configurations=observations,new_references=0,new_production_changes=0)
if __name__=='__main__':print(json.dumps(inspect()))
