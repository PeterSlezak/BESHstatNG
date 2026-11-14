"""Compile/replay ONLY current sources against unchanged pre-edit/A-G inputs.

Original references are read, never executed or regenerated. Each corpus keeps
its exact frozen recorder/version and its distinct provenance/configuration.
"""
import shutil,csv
from concurrent.futures import ThreadPoolExecutor
from common import *

def compile_udf(stage,config):
 run=PRIVATE/stage/config;folder=run/'udf';folder.mkdir(exist_ok=False)
 for n in ['BESHStatNG.dll','BESHStat.Core.dll','BESHStat.Excel.Common.dll','ExcelDna.Integration.dll']:shutil.copy2(run/'lib'/n,folder/n)
 sources=[ROOT/r['source']['path'] for r in json.loads((ROOT/'evidence/04A38/preedit/entry-source-snapshot.json').read_bytes())['files']]
 sources += [ROOT/'BESHstatNG/src/DM/WindowsSpreadsheetTransport.vb',ROOT/'BESHstatNG/src/RegModels/MixedModels/MixedModelFrontEndHelpers.vb']
 sources += [ROOT/'evidence/04A38/preedit/native-safety'/n for n in ['NativeBoundary38.vb','DependencySlices38.vb','FixtureInputs38.vb']]
 facade=Path('C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8/Facades/netstandard.dll')
 refs=[folder/n for n in ['BESHStatNG.dll','BESHStat.Core.dll','BESHStat.Excel.Common.dll','ExcelDna.Integration.dll']]+[facade]
 r=command(stage+'-'+config+'-safe-udf-compile',[compiler('vbc.exe'),'/nologo','/target:library','/rootnamespace:BESHStatNG','/optioncompare:text','/optioninfer+','/optionexplicit+',
  '/imports:System,System.Collections,System.Collections.Generic,System.Diagnostics,System.Linq,System.Globalization','/optimize'+('+' if config=='Release' else '-'),'/out:'+str(folder/'BESHStat.Core.Tests.dll'),
  '/reference:System.Core.dll','/reference:System.dll','/reference:System.Data.dll','/reference:System.Xml.Linq.dll',*['/reference:'+str(p) for p in refs],*map(str,sources)]);assert r['exit_code']==0
 return folder,facade,dict(command=clean(r),sources=[pin(p) for p in sources],assemblies=[library_pin(p) for p in sorted(folder.glob('*.dll'))])

def main():
 stage=sys.argv[1];lanes=[];compiled=[]
 pre=ROOT/'evidence/04A38/preedit';supp=ROOT/'evidence/04A38/late-archived-entry-supplement'
 for config in ['Debug','Release']:
  folder,facade,proof=compile_udf(stage,config);compiled.append(dict(configuration=config,**proof))
  packets=[('preedit-main',pre/'frozen-v2',pre/'frozen-v2/Capture38.cs',pre/'frozen-v2/cases.jsonl',pre/'frozen-v2'/('original-'+config+'.tsv'),24732)]
  for name,count in [('public-options-v1',2508),('public-primitives-v1',226),('model-inputs-v2',882),('math-edges-v1',618)]:
   p=pre/'attempts'/name;source=p/'Capture38.cs' if name in ['model-inputs-v2','math-edges-v1'] else pre/'frozen-v2/Capture38.cs'
   packets.append((name,p,source,p/'cases.jsonl',p/('capture-'+config+'-original-v1.tsv'),count))
  for authority,directory,recorder,count,mode in [('A','v1','LateCapture38.cs',432,'plain-v2'),('B','v2-gee-profile','GeeCapture38-v2.cs',12,'plain-v2'),('C','v3-mmrm-headered-profile','MmrmCapture38.cs',2,'plain'),('D','v4-mmrm-duplicate-guards','MmrmCapture38.cs',4,'plain'),('E','v5-mmrm-unmatched-profile','MmrmCapture38.cs',2,'plain'),('F','v6-mmrm-intercept-group','MmrmCapture38.cs',2,'plain'),('G','v7-consolidated-gee-spec-mmrm-wide','MmrmCapture38-v2.cs',4,'plain')]:
   p=supp/directory;original=p/('original-results-v2' if authority=='G' else 'original-results')/config/mode/'results.tsv'
   packets.append((authority,p,p/recorder,p/('inputs-'+config+'.jsonl'),original,count))
  for name,p,source,inputs,original,count in packets:
   exe=folder/(name+'-current.exe')
   r=command(stage+'-'+config+'-'+name+'-recorder',[compiler('csc.exe'),'/nologo','/target:exe','/reference:System.Core.dll','/reference:System.Web.Extensions.dll','/reference:'+str(folder/'BESHStat.Core.dll'),'/reference:'+str(facade),'/out:'+str(exe),str(source)]);assert r['exit_code']==0
   lanes.append((config,name,folder,exe,source,inputs,original,count))
 def run(lane):
  config,name,folder,exe,source,inputs,original,count=lane;out=HERE/'captures'/stage/config/name;out.mkdir(parents=True,exist_ok=False)
  args=[str(exe),str(folder),str(inputs),str(out/'results.tsv'),str(out/'execution.json')]
  if name in list('ABCDEFG'):args.append('current')
  r=command(stage+'-'+config+'-'+name+'-current',args,env={'BESHSTAT_LATE38_CONFIGURATION':config});assert r['exit_code']==0
  now=(out/'results.tsv').read_bytes();equal=now==original.read_bytes();record=dict(configuration=config,corpus=name,cases=count,exact=equal,original=pin(original),current=pin(out/'results.tsv'),inputs=pin(inputs),recorder=pin(source),command=clean(r),original_queries=0)
  save(('captures/'+stage+'/'+config+'/'+name+'/comparison.json'),record)
  return record
 with ThreadPoolExecutor(max_workers=4) as pool:results=list(pool.map(run,lanes))
 save('compatibility-'+stage+'.json',dict(status='CURRENT_ONLY_FROZEN_COMPARISONS',compilation=compiled,results=results,all_exact=all(r['exact'] for r in results),totals_per_configuration={c:sum(r['cases'] for r in results if r['configuration']==c) for c in ['Debug','Release']},new_original_queries=0,native='NOT_RUN',science='NOT_GOLDEN'))
 assert all(r['exact'] for r in results),'Mismatch: preserve original/current outputs and investigate; never change the oracle'
 print('Current preedit28966 + separate late458 EXACT each configuration',stage)
if __name__=='__main__':main()
