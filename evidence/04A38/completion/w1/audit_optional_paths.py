"""Inspect existing optional argument bytes only; no new reference materialization."""
import base64,csv,json,re,sys
from collections import Counter
from pathlib import Path
HERE=Path(__file__).resolve().parent;ROOT=HERE.parents[3]
sys.path.insert(0,str(ROOT/'evidence/04A38/continuation/v1'))
from audit_frozen_branches import children
def main():
 requests={'BESH.MULTI.KMEANS_FIT':['startingCenters'],'BESH.REGR.GEE_LSMESTIMATE':['spec','at'],'BESH.REGR.MMRM_LSMESTIMATE':['spec','at']}
 compiled=json.loads((ROOT/'evidence/04A38/preedit/compiled-Debug.json').read_bytes())
 exports={r['effective_excel_name']:r for r in compiled['exports']}
 k=[n for n in exports if 'KMEANS' in n and n.endswith('FIT')];assert len(k)==1;requests[k[0]]=requests.pop('BESH.MULTI.KMEANS_FIT') if k[0]!='BESH.MULTI.KMEANS_FIT' else requests[k[0]]
 folders=['frozen-v2','attempts/public-options-v1','attempts/public-primitives-v1','attempts/model-inputs-v2','attempts/math-edges-v1'];observations={};witnesses=[]
 for config in ['Debug','Release']:
  counts={key:{a:Counter() for a in args} for key,args in requests.items()}
  for relative in folders:
   folder=ROOT/'evidence/04A38/preedit'/relative;cases={r['case_id']:r for r in map(json.loads,(folder/'cases.jsonl').read_bytes().splitlines())}
   output=folder/('original-'+config+'.tsv' if relative=='frozen-v2' else 'capture-'+config+'-original-v1.tsv')
   with output.open(encoding='utf-8',newline='') as f:
    for row in csv.DictReader(f,delimiter='\t'):
     c=cases[row['case_id']];target=c.get('target')
     if target not in requests:continue
     args=children(row['byref_arguments']);names=[a['name'] for a in exports[target]['arguments']]
     if not args:continue
     selected={name:args['values'][names.index(name)] for name in requests[target]}
     for name,value in selected.items():counts[target][name][value]+=1
     if 'KMEANS' in target and row['result'].startswith('HKMEANS'):
      witnesses.append(dict(configuration=config,case_id=c['case_id'],culture=c['culture'],target=target,outcome=row['outcome'],input=row['byref_arguments'],result=row['result']))
  observations[config]={t:{a:dict(v) for a,v in attrs.items()} for t,attrs in counts.items()}
 with (HERE/'optional-path-observations.json').open('x',encoding='utf-8') as f:json.dump(dict(status='READ_ONLY_EXISTING_BYTES_NO_NEW_INPUTS',configurations=observations,successful_kmeans_witnesses=witnesses,new_reference_executions=0),f,indent=2)
 print(json.dumps(dict(arguments={t:list(v) for t,v in observations['Debug'].items()},kmeans=observations['Debug'][k[0]],successful_kmeans_witnesses=len(witnesses)),ensure_ascii=True))
if __name__=='__main__':main()
