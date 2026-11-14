"""Resume current-only capture after preserved recorder/path selection failures."""
from capture_current import *

def main():
 config=sys.argv[1];run=RUNROOT/'completion-w2/profiles-v3'/config;folder=run/'udf-v2';runner=m.Runner(RUNROOT)
 for n in ['BESHStatNG.dll','BESHStat.Core.dll','BESHStat.Excel.Common.dll','ExcelDna.Integration.dll']:assert (folder/n).read_bytes()==(run/'lib'/n).read_bytes()
 facade=Path('C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8/Facades/netstandard.dll')
 base=ROOT/'evidence/04A38/late-archived-entry-supplement';results=[]
 for label,name,recorder in [('B','v2-gee-profile','GeeCapture38-v2.cs'),('C','v3-mmrm-headered-profile','MmrmCapture38.cs'),('D','v4-mmrm-duplicate-guards','MmrmCapture38.cs'),('E','v5-mmrm-unmatched-profile','MmrmCapture38.cs'),('F','v6-mmrm-intercept-group','MmrmCapture38.cs'),('G','v7-consolidated-gee-spec-mmrm-wide','MmrmCapture38-v2.cs')]:
  packet=base/name;out=HERE/'profile-current-results-v2'/config/label;exe=folder/(label+'-current.exe');cmds=[]
  if label!='B':
   cr=runner.command('w2-profile-v3-'+config+'-'+label+'-recorder',[compiler('csc.exe'),'/nologo','/target:exe','/reference:System.Core.dll','/reference:System.Web.Extensions.dll','/reference:'+str(folder/'BESHStat.Core.dll'),'/reference:'+str(facade),'/out:'+str(exe),str(packet/recorder)]);assert cr['exit_code']==0;cmds.append(clean(cr))
   out.mkdir(parents=True,exist_ok=False)
   er=runner.command('w2-profile-v3-'+config+'-'+label,[str(exe),str(folder),str(packet/('inputs-'+config+'.jsonl')),str(out/'results.tsv'),str(out/'execution.json'),'current'],env={'BESHSTAT_LATE38_CONFIGURATION':config});assert er['exit_code']==0;cmds.append(clean(er))
  original=packet/('original-results-v2' if label=='G' else 'original-results')/config/('plain-v2' if label=='B' else 'plain')/'results.tsv'
  equal=(out/'results.tsv').read_bytes()==original.read_bytes()
  results.append(dict(authority=label,original=pin(original),current=pin(out/'results.tsv'),exact=equal,commands=cmds,recorder=pin(packet/recorder),inputs=pin(packet/('inputs-'+config+'.jsonl'))))
  assert equal,'Frozen original/current mismatch: '+label
 with (HERE/('profile-compatibility-v3-'+config+'.json')).open('x',encoding='utf-8') as f:json.dump(dict(configuration=config,results=results,all_exact=True,new_original_queries=0,native='NOT_RUN',earlier_attempts='Initial invalid B recorder selected then corrected; reference-path lookup used plain rather than protected plain-v2. No oracle changed and completed B current execution reused.'),f,indent=2)
 print('B-G frozen current profile cases EXACT',config)
if __name__=='__main__':main()
