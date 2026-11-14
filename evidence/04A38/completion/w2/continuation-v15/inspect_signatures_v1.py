"""Passive current-only complete signatures; independently retain identical measured CIL."""
from common import *
folder=PRIVATE/'signatures-v1';folder.mkdir(exist_ok=False)
exe=folder/'MethodMetadata38.exe'
r=command('signatures-tool-v1',[compiler('csc.exe'),'/nologo','/target:exe','/reference:System.Core.dll','/reference:System.Web.Extensions.dll','/out:'+str(exe),str(HERE/'MethodMetadata-v12.cs')]);assert r['exit_code']==0
commands=[clean(r)];rows=[]
for cfg in ['Debug','Release']:
    target=HERE/'metadata-signatures-v1'/('current-'+cfg+'.json');target.parent.mkdir(exist_ok=True)
    r=command('signatures-current-'+cfg+'-v1',[str(exe),str(PRIVATE/'packet-v2'/cfg/'lib'),str(target),str(HERE/'native-metadata-owners-v3.txt')]);commands.append(clean(r));assert r['exit_code']==0
    before=json.loads((HERE/('metadata-all/packet-v2-context-v5/current-'+cfg+'.json')).read_bytes())
    now=json.loads(target.read_bytes())
    stripped=[{k:v for k,v in method.items() if k!='signature'} for method in now['methods']]
    assert stripped==before['methods'] and now['types']==before['types']
    assert all(method['signature']['return_type'] is not None and all(p['type'] is not None for p in method['signature']['parameters']) for method in now['methods'])
    rows.append(dict(configuration=cfg,current=pin(target),same_CIL_authority=pin(HERE/('metadata-all/packet-v2-context-v5/current-'+cfg+'.json')),methods=len(now['methods']),all_return_parameter_types_measured=True,all_CIL_exception_edges_unchanged=True))
save('compiled-full-signatures-v1.json',dict(rows=rows,source=pin(HERE/'MethodMetadata-v12.cs'),tool=pin(exe),commands=commands,metadata_only=True,original_queries=0))
print('Actual current constructed return/parameter/generic/delegate signatures measured in Debug and Release')
