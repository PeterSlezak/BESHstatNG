"""Re-execute only CURRENT432 cases; original Approval A remains untouched."""
from capture_current import *
def main():
 config=sys.argv[1];folder=RUNROOT/'completion-w2/profiles-v3'/config/'udf-v2';a=ROOT/'evidence/04A38/late-archived-entry-supplement/v1'
 facade=Path('C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8/Facades/netstandard.dll');runner=m.Runner(RUNROOT);exe=folder/'A-current.exe'
 c=runner.command('w2-profile-'+config+'-A-recorder',[compiler('csc.exe'),'/nologo','/target:exe','/reference:System.Core.dll','/reference:System.Web.Extensions.dll','/reference:'+str(folder/'BESHStat.Core.dll'),'/reference:'+str(facade),'/out:'+str(exe),str(a/'LateCapture38.cs')]);assert c['exit_code']==0
 out=HERE/'profile-current-results-v2'/config/'A';out.mkdir()
 r=runner.command('w2-profile-'+config+'-A',[str(exe),str(folder),str(a/('inputs-'+config+'.jsonl')),str(out/'results.tsv'),str(out/'execution.json'),'current'],env={'BESHSTAT_LATE38_CONFIGURATION':config});assert r['exit_code']==0
 original=a/'original-results'/config/'plain-v2/results.tsv';equal=original.read_bytes()==(out/'results.tsv').read_bytes()
 with (HERE/('A-compatibility-'+config+'.json')).open('x',encoding='utf-8') as f:json.dump(dict(configuration=config,unique_cases=432,original=pin(original),current=pin(out/'results.tsv'),exact=equal,commands=[clean(c),clean(r)],new_original_queries=0),f,indent=2)
 assert equal,'Approval A public outcomes differ'
 print('A432 current EXACT',config)
if __name__=='__main__':main()
