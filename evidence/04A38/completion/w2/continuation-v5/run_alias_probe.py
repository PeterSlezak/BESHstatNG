from common import *
stage,role=sys.argv[1:3]
folder=PRIVATE/'alias-probe';folder.mkdir(exist_ok=True);exe=folder/'AliasProbe.exe'
if not exe.exists():
    r=command('alias-probe-compile',[compiler('csc.exe'),'/nologo','/target:exe','/reference:System.Core.dll','/reference:System.Web.Extensions.dll','/out:'+str(exe),str(HERE/'AliasProbe.cs')]);assert r['exit_code']==0
records=[]
for c in ['Debug','Release']:
    dst=HERE/('alias-probe-'+role+'-'+c+'.json')
    r=command('alias-probe-'+role+'-'+c,[str(exe),str(PRIVATE/stage/c/'lib'),str(dst)]);records.append(clean(r));assert r['exit_code']==0
    data=json.loads(dst.read_bytes())
    if role=='after':assert all(row['output']==17 for row in data['cases'])
save('alias-probe-'+role+'-commands.json',dict(commands=records,original_queries=0,
    scope='Current implementation only; static original body places the sole output reset before the converter callback; no original source is compiled or semantically executed'))
