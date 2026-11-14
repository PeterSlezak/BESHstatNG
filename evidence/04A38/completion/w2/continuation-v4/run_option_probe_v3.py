from common import *
folder=PRIVATE/'option-probe-v3';folder.mkdir();exe=folder/'OptionProbe.exe';r=command('option-probe-v3-compile',[compiler('csc.exe'),'/nologo','/reference:System.Core.dll','/reference:System.Web.Extensions.dll','/out:'+str(exe),str(HERE/'OptionBoundaryProbe-v2.cs')]);assert r['exit_code']==0;records=[clean(r)]
for c in ['Debug','Release']:
 out=HERE/('option-probe-v3-'+c+'.json');r=command('option-probe-v3-'+c,[str(exe),str(PRIVATE/'parametric-v4'/c/'lib'),str(HERE/'option-probe-packets.json'),str(out)]);records.append(clean(r));assert r['exit_code']==0
save('option-probe-v3-commands.json',dict(commands=records,current_only=True,original_calls=0,prior_v2='FileNotFoundException: overbroad orchestration filename replacement requested nonexistent option-probe-v2-packets.json; original sealed current-only packet retained unchanged'))
