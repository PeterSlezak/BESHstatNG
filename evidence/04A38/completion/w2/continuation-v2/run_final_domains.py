import shutil
from common import *
stage=sys.argv[1];folder=PRIVATE/stage/'text-domains-final';folder.mkdir();records=[]
vb=Path('C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8/Microsoft.VisualBasic.dll');facade=vb.parent/'Facades/netstandard.dll'
for config in ['Debug','Release']:
 target=folder/config;target.mkdir()
 for n in ['BESHStat.Core.dll','BESHStat.Excel.Common.dll']:shutil.copy2(PRIVATE/stage/config/'lib'/n,target/n)
 exe=target/'TextDomainProbe.exe'
 r=command(stage+'-text-domain-v2-'+config+'-compile',[compiler('csc.exe'),'/nologo','/target:exe','/reference:System.Core.dll','/reference:System.Web.Extensions.dll','/reference:'+str(target/'BESHStat.Excel.Common.dll'),'/reference:'+str(vb),'/reference:'+str(facade),'/out:'+str(exe),str(HERE/'TextDomainProbe-v2.cs')]);assert r['exit_code']==0
 output=HERE/('text-domains-'+stage+'-final-'+config+'.json');r2=command(stage+'-text-domain-v2-'+config,[str(exe),str(HERE/'text-call-sites-final.json'),str(output)]);assert r2['exit_code']==0
 records.append(dict(configuration=config,compile=clean(r),run=clean(r2),result=pin(output),assembly=library_pin(target/'BESHStat.Excel.Common.dll')))
save('text-domain-execution-'+stage+'-final.json',dict(commands=records,source=pin(HERE/'TextDomainProbe-v2.cs'),policy=pin(ROOT/'BESHStat.Excel.Common/src/Analysis/LegacyUdfText38.vb'),production_policy_changed=False,original_queries=0))
