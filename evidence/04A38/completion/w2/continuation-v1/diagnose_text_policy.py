"""Diagnose, do not normalize away, the retained current runtime control failure."""
import shutil
from common import *
folder=PRIVATE/'text-policy-v2';folder.mkdir();records=[]
vb=Path('C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8/Microsoft.VisualBasic.dll')
facade=vb.parent/'Facades/netstandard.dll'
for config in ['Debug','Release']:
 target=folder/config;target.mkdir()
 for n in ['BESHStat.Core.dll','BESHStat.Excel.Common.dll']:shutil.copy2(PRIVATE/'p1-v5'/config/'lib'/n,target/n)
 exe=target/'CulturePolicyProbeV2.exe'
 r=command('text-policy-v2-'+config+'-compile',[compiler('csc.exe'),'/nologo','/target:exe','/reference:'+str(target/'BESHStat.Excel.Common.dll'),'/reference:'+str(vb),'/reference:'+str(facade),'/out:'+str(exe),str(HERE/'CulturePolicyProbe-v2.cs')]);assert r['exit_code']==0
 run=command('text-policy-v2-'+config,[str(exe)]);records.append(dict(configuration=config,compile=clean(r),run=clean(run),stdout=(RUNROOT/'commands'/('w2c1-text-policy-v2-'+config)/'stdout.log').read_text()))
save('options-policy-diagnostic.json',dict(status='INVESTIGATIVE_CURRENT_POLICY_NONPASS_NOT_NEW_ARCHIVED_REFERENCE',records=records,new_original_BESH_queries=0,production_changed=False,first_failure='w2c1-text-policy-Debug exit3762504530 preserved unhandled assertion; v2 emits all mismatches',next_action='Keep affected option bodies OPEN; inspect null versus empty normalization and original/runtime comparison semantics before any closure or compatible source change.'))
for r in records:print(r['configuration'],r['run']['exit_code'],r['stdout'])
