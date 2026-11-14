from common import *
folder=PRIVATE/'option-probe';folder.mkdir();exe=folder/'OptionProbe.exe';r=command('option-probe-compile',[compiler('csc.exe'),'/nologo','/reference:System.Core.dll','/reference:System.Web.Extensions.dll','/out:'+str(exe),str(HERE/'OptionBoundaryProbe.cs')]);assert r['exit_code']==0;records=[clean(r)]
for c in ['Debug','Release']:
 out=HERE/('option-probe-'+c+'.json');r=command('option-probe-'+c,[str(exe),str(PRIVATE/'parametric-v4'/c/'lib'),str(HERE/'option-probe-packets.json'),str(out)]);records.append(clean(r));assert r['exit_code']==0
save('option-probe-commands.json',dict(commands=records,current_only=True,original_calls=0))
