"""Current-only public UDF verification against immutable original packets.

No original source/model/query is executed by this script. Existing frozen
recorders are compiled unchanged against fresh current libraries.
"""
import sys,json,shutil
from pathlib import Path
HERE=Path(__file__).resolve().parent;ROOT=HERE.parents[3]
G=ROOT/'evidence/04A38/late-archived-entry-supplement/v7-consolidated-gee-spec-mmrm-wide';sys.path.insert(0,str(G))
from common import RUNROOT,m,clean,pin,compiler,library_pin,sha

def main():
 config=sys.argv[1];run=RUNROOT/'completion-w2/profiles-v3'/config;folder=run/'udf-v2';folder.mkdir(exist_ok=False);runner=m.Runner(RUNROOT)
 for n in ['BESHStatNG.dll','BESHStat.Core.dll','BESHStat.Excel.Common.dll','ExcelDna.Integration.dll']:shutil.copy2(run/'lib'/n,folder/n)
 paths=[ROOT/r['source']['path'] for r in json.loads((ROOT/'evidence/04A38/preedit/entry-source-snapshot.json').read_bytes())['files']]
 paths+=[ROOT/'BESHstatNG/src/DM/WindowsSpreadsheetTransport.vb',ROOT/'BESHstatNG/src/RegModels/MixedModels/MixedModelFrontEndHelpers.vb']
 paths+=[ROOT/'evidence/04A38/preedit/native-safety'/n for n in ['NativeBoundary38.vb','DependencySlices38.vb','FixtureInputs38.vb']]
 facade=Path('C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8/Facades/netstandard.dll')
 args=[compiler('vbc.exe'),'/nologo','/target:library','/rootnamespace:BESHStatNG','/optioncompare:text','/optioninfer+','/optionexplicit+',
  '/imports:System,System.Collections,System.Collections.Generic,System.Diagnostics,System.Linq,System.Globalization','/optimize'+('+' if config=='Release' else '-'),
  '/out:'+str(folder/'BESHStat.Core.Tests.dll'),'/reference:System.Core.dll','/reference:System.dll','/reference:System.Data.dll','/reference:System.Xml.Linq.dll',
  *['/reference:'+str(p) for p in [folder/n for n in ['BESHStatNG.dll','BESHStat.Core.dll','BESHStat.Excel.Common.dll','ExcelDna.Integration.dll']]+[facade]],*map(str,paths)]
 r=runner.command('w2-profile-v2-udf-'+config,args);assert r['exit_code']==0
 records=[]
 old=ROOT/'evidence/04A38/late-archived-entry-supplement'
 for label,packet,recorder in [('B',old/'v2-gee-profile','GeeCapture38-v2.cs'),('C',old/'v3-mmrm-headered-profile','MmrmCapture38.cs'),('D',old/'v4-mmrm-duplicate-guards','MmrmCapture38.cs'),('E',old/'v5-mmrm-unmatched-profile','MmrmCapture38.cs'),('F',old/'v6-mmrm-intercept-group','MmrmCapture38.cs'),('G',G,'MmrmCapture38-v2.cs')]:
  source=packet/recorder;exe=folder/(label+'-current.exe')
  cr=runner.command('w2-profile-v2-'+config+'-'+label+'-recorder',[compiler('csc.exe'),'/nologo','/target:exe','/reference:System.Core.dll','/reference:System.Web.Extensions.dll','/reference:'+str(folder/'BESHStat.Core.dll'),'/reference:'+str(facade),'/out:'+str(exe),str(source)]);assert cr['exit_code']==0
  out=HERE/'profile-current-results-v2'/config/label;out.mkdir(parents=True,exist_ok=False)
  er=runner.command('w2-profile-v2-'+config+'-'+label,[str(exe),str(folder),str(packet/('inputs-'+config+'.jsonl')),str(out/'results.tsv'),str(out/'execution.json'),'current'],env={'BESHSTAT_LATE38_CONFIGURATION':config});assert er['exit_code']==0
  original=packet/('original-results-v2' if label=='G' else 'original-results')/config/'plain/results.tsv'
  assert original.exists(),str(original.relative_to(ROOT))
  equal=original.read_bytes()==(out/'results.tsv').read_bytes()
  records.append(dict(authority=label,recorder=pin(source),inputs=pin(packet/('inputs-'+config+'.jsonl')),original=pin(original),current=pin(out/'results.tsv'),exact=equal,commands=[clean(cr),clean(er)]))
 result=dict(configuration=config,status='CURRENT_ONLY_FROZEN_PROFILE_COMPARISON',compilation=clean(r),source_files=[pin(p) for p in paths],assemblies=[library_pin(p) for p in folder.glob('*.dll')],results=records,all_exact=all(r['exact'] for r in records),native='NOT_RUN',new_original_queries=0)
 with (HERE/('profile-compatibility-v2-'+config+'.json')).open('x',encoding='utf-8') as f:json.dump(result,f,indent=2)
 assert result['all_exact'],'Current differs from frozen original; do not modify the reference'
 print('B-G current profile cases EXACT',config)
if __name__=='__main__':main()
