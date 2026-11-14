"""Replay exactly the five genuine pre-edit packets on CURRENT code only."""
from concurrent.futures import ThreadPoolExecutor
from capture_current import *

def main():
 runner=m.Runner(RUNROOT);facade=Path('C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8/Facades/netstandard.dll')
 packets=[('frozen-v2',ROOT/'evidence/04A38/preedit/frozen-v2')]+[(n,ROOT/'evidence/04A38/preedit/attempts'/n) for n in ['public-options-v1','public-primitives-v1','model-inputs-v2','math-edges-v1']]
 for config in ['Debug','Release']:
  folder=RUNROOT/'completion-w2/profiles-v3'/config/'udf-v2'
  for name,packet in packets:
   source=packet/'Capture38.cs' if name in ['frozen-v2','model-inputs-v2','math-edges-v1'] else ROOT/'evidence/04A38/preedit/frozen-v2/Capture38.cs'
   exe=folder/(name+'-current.exe')
   r=runner.command('w2-preedit-'+config+'-'+name+'-recorder',[compiler('csc.exe'),'/nologo','/target:exe','/reference:System.Core.dll','/reference:System.Web.Extensions.dll','/reference:'+str(folder/'BESHStat.Core.dll'),'/reference:'+str(facade),'/out:'+str(exe),str(source)]);assert r['exit_code']==0
 def run(item):
  config,name,packet=item;folder=RUNROOT/'completion-w2/profiles-v3'/config/'udf-v2';out=HERE/'preedit-current-results'/config/name;out.mkdir(parents=True,exist_ok=False)
  r=runner.command('w2-preedit-'+config+'-'+name,[str(folder/(name+'-current.exe')),str(folder),str(packet/'cases.jsonl'),str(out/'results.tsv'),str(out/'execution.json')]);assert r['exit_code']==0
  original=packet/('original-'+config+'.tsv' if name=='frozen-v2' else 'capture-'+config+'-original-v1.tsv')
  raw=(out/'results.tsv').read_bytes();equal=raw==original.read_bytes()
  result=dict(configuration=config,packet=name,unique_cases=len((packet/'cases.jsonl').read_bytes().splitlines()),original=pin(original),current=pin(out/'results.tsv'),exact=equal,command=clean(r),new_original_queries=0)
  with (out/'comparison.json').open('x',encoding='utf-8') as f:json.dump(result,f,indent=2)
  return result
 with ThreadPoolExecutor(max_workers=4) as pool:results=list(pool.map(run,[(c,n,p) for c in ['Debug','Release'] for n,p in packets]))
 with (HERE/'preedit-compatibility.json').open('x',encoding='utf-8') as f:json.dump(dict(results=results,all_exact=all(r['exact'] for r in results),unique_cases_per_configuration={c:sum(r['unique_cases'] for r in results if r['configuration']==c) for c in ['Debug','Release']},native='NOT_RUN',new_original_queries=0),f,indent=2)
 assert all(r['exact'] for r in results),'Current/pre-edit mismatch; retain both exact outcomes'
 print('Original pre-edit28966/configuration EXACT on current code')
if __name__=='__main__':main()
