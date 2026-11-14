"""Complete real current and passive retained-original CIL. No original calls."""
from common import *
stage=sys.argv[1];folder=PRIVATE/stage/'metadata-all-v2';folder.mkdir(exist_ok=False)
exe=folder/'MethodMetadata38.exe'
result=command(stage+'-metadata-v2-tool',[compiler('csc.exe'),'/nologo','/target:exe','/reference:System.Core.dll','/reference:System.Web.Extensions.dll','/out:'+str(exe),str(HERE/'MethodMetadata-v10.cs')])
assert result['exit_code']==0
receipts=[clean(result)];files=[]
for configuration in ['Debug','Release']:
    for origin,library in [('current',PRIVATE/stage/configuration/'lib'),('original',RUNROOT/'consolidated38-v7/original-build'/configuration)]:
        output=HERE/'metadata-all'/(stage+'-context-v2')/(origin+'-'+configuration+'.json');output.parent.mkdir(parents=True,exist_ok=True)
        result=command(stage+'-metadata-v2-'+origin+'-'+configuration,[str(exe),str(library),str(output),str(HERE/'native-metadata-owners-v2.txt')])
        receipts.append(clean(result));assert result['exit_code']==0
        files.append(pin(output))
save('metadata-all-v2-'+stage+'.json',dict(commands=receipts,files=files,original_queries=0,original_activity='PASSIVE_METADATA_ONLY_NO_METHOD_OR_TYPE_INITIALIZER_EXECUTED'))
