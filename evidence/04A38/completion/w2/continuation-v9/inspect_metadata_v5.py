"""Fresh current CIL; reuse authenticated original metadata, never original execution."""
from common_v2 import *
stage=sys.argv[1];folder=PRIVATE/stage/'metadata-all-v5';folder.mkdir(exist_ok=False);exe=folder/'MethodMetadata38.exe'
r=command(stage+'-metadata-v5-tool',[compiler('csc.exe'),'/nologo','/target:exe','/reference:System.Core.dll','/reference:System.Web.Extensions.dll','/out:'+str(exe),str(HERE/'MethodMetadata-v11.cs')]);assert r['exit_code']==0
commands=[clean(r)];files=[];originals=[]
for config in ['Debug','Release']:
    target=HERE/'metadata-all'/(stage+'-context-v5');target.mkdir(parents=True,exist_ok=True)
    output=target/('current-'+config+'.json');r=command(stage+'-metadata-v5-current-'+config,[str(exe),str(PRIVATE/stage/config/'lib'),str(output),str(HERE/'native-metadata-owners-v3.txt')]);commands.append(clean(r));assert r['exit_code']==0;files.append(pin(output))
    before=PREVIOUS/'metadata-all/packet-v1-context-v3'/('original-'+config+'.json');data=before.read_bytes()
    # Its exact immutable original authority was authenticated by the C8 reader at entry.
    with (target/('original-'+config+'.json')).open('xb') as f:f.write(data)
    originals.append(dict(before=pin(before),copy=pin(target/('original-'+config+'.json'))))
save('metadata-all-v5-'+stage+'.json',dict(commands=commands,current=files,original=originals,original_queries=0,original_activity='EXACT_REUSE_AUTHENTICATED_RETAINED_C8_METADATA_NO_ORIGINAL_DLL_LOAD_OR_EXECUTION'))
print('Fresh current metadata; original measurements copied byte-exact')
