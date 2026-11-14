from common import *
stage=sys.argv[1];folder=PRIVATE/('inversion-'+stage);folder.mkdir();exe=folder/'Probe.exe';vb=Path('C:/Windows/Microsoft.NET/Framework/v4.0.30319/Microsoft.VisualBasic.dll')
if not vb.exists():vb=Path('C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8/Microsoft.VisualBasic.dll')
r=command('inversion-'+stage+'-compile',[compiler('csc.exe'),'/nologo','/reference:System.Web.Extensions.dll','/reference:'+str(vb),'/out:'+str(exe),str(HERE/'InversionProbe.cs')]);assert r['exit_code']==0;records=[clean(r)]
for c in ['Debug','Release']:
 lib=RUNROOT/'completion-w2-cont3/names-v3'/c/'lib' if stage=='before' else PRIVATE/stage/c/'lib'
 out=HERE/('inversion-'+stage+'-'+c+'.json');r=command('inversion-'+stage+'-'+c,[str(exe),str(lib),str(out)]);records.append(clean(r));assert r['exit_code']==0
save('inversion-'+stage+'-commands.json',dict(commands=records,source=pin(HERE/'InversionProbe.cs'),original_calls=0))
