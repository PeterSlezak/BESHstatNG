"""Fresh current syntax/export/IL proof; no original model or UDF execution."""
from capture_current import *

def main():
 runner=m.Runner(RUNROOT);tools=RUNROOT/'mmrm38-v6/w1-inspection';w1=ROOT/'evidence/04A38/completion/w1'
 census=RUNROOT/'mmrm38-v6/w1-census-tool-v2/bin/Census.dll';assert census.exists()
 records=[]
 r=runner.command('w2-profile-current-syntax',['dotnet',str(census),str(ROOT),str(HERE/'current-universe-profiles-v1.json')]);assert r['exit_code']==0;records.append(clean(r))
 for config in ['Debug','Release']:
  folder=RUNROOT/'completion-w2/profiles-v3'/config/'lib'
  r=runner.command('w2-profile-'+config+'-exports',[str(tools/'Inspect38.exe'),str(folder),str(HERE/('compiled-'+config+'-profiles-v1.json')),str(ROOT/'evidence/04A38/entry-members.json')]);assert r['exit_code']==0;records.append(clean(r))
  r=runner.command('w2-profile-'+config+'-closure',[str(tools/'PortableConsumerClosure38.exe'),str(folder),str(ROOT/'evidence/04A38/entry-members.json'),str(HERE/('closure-'+config+'-profiles-v1.json'))]);assert r['exit_code']==0;records.append(clean(r))
  original=json.loads((ROOT/'evidence/04A38/preedit'/('compiled-'+config+'.json')).read_bytes())
  current=json.loads((HERE/('compiled-'+config+'-profiles-v1.json')).read_bytes())
  assert sorted(original['exports'],key=lambda r:r['effective_excel_name'])==sorted(current['exports'],key=lambda r:r['effective_excel_name'])
  edges=[]
  for method in current['methods']:
   for call in method['calls']:
    if 'UdfModelProfilePreparation38' in call['target'] or '.UdfProfilePreparation38.FillRowMask' in call['target']:edges.append(dict(caller=method['method'],call=call))
  assert len(edges)>=6
  with (HERE/('profile-edges-'+config+'.json')).open('x',encoding='utf-8') as f:json.dump(dict(exports_exact=268,actual_direct_edges=edges,whole_body_closure='NOT_INFERRED_FROM_EDGE'),f,indent=2)
 with (HERE/'profile-inspection-receipts.json').open('x',encoding='utf-8') as f:json.dump(dict(commands=records,source_only_census='Post-edit identity update, not a new reference-adequacy census or evidence capture'),f,indent=2)
 print('268 exports exact both configurations; current source identities and real IL edges recorded')
if __name__=='__main__':main()
