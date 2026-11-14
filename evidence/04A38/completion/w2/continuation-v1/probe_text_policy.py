"""Current runtime policy controls, not archived BESH reference cases."""
import shutil
from common import *
folder=PRIVATE/'text-policy';folder.mkdir();runner=m.Runner(RUNROOT);records=[]
vb=Path('C:/Windows/Microsoft.NET/Framework64/v4.0.30319/Microsoft.VisualBasic.dll')
if not vb.exists():vb=Path('C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8/Microsoft.VisualBasic.dll')
facade=Path('C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8/Facades/netstandard.dll')
for config in ['Debug','Release']:
 target=folder/config;target.mkdir()
 for n in ['BESHStat.Core.dll','BESHStat.Excel.Common.dll']:shutil.copy2(PRIVATE/'p1-v5'/config/'lib'/n,target/n)
 exe=target/'CulturePolicyProbe.exe'
 r=command('text-policy-'+config+'-compile',[compiler('csc.exe'),'/nologo','/target:exe','/reference:'+str(target/'BESHStat.Excel.Common.dll'),'/reference:'+str(vb),'/reference:'+str(facade),'/out:'+str(exe),str(HERE/'CulturePolicyProbe.cs')]);assert r['exit_code']==0
 run=command('text-policy-'+config,[str(exe)]);assert run['exit_code']==0
 records.append(dict(configuration=config,compile=clean(r),run=clean(run),executable=library_pin(exe),Common=library_pin(target/'BESHStat.Excel.Common.dll')))
save('options-policy-probe.json',dict(scope='CURRENT_DOTNET_RUNTIME_POLICY_CONTROL_NOT_ORIGINAL_UDF_REFERENCE',records=records,source=pin(HERE/'CulturePolicyProbe.cs'),new_original_BESH_queries=0))
