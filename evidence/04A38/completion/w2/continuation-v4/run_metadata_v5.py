from common import *
stage=sys.argv[1];out=PRIVATE/stage/'metadata-v5';out.mkdir();exe=out/'MethodMetadata38.exe'
r=command(stage+'-metadata-v5-compile',[compiler('csc.exe'),'/nologo','/target:exe','/reference:System.Core.dll','/reference:System.Web.Extensions.dll','/out:'+str(exe),str(HERE/'MethodMetadata-v5.cs')]);receipts=[clean(r)];assert r['exit_code']==0
files=[]
for role in ['original','current']:
 for config in ['Debug','Release']:
  lib=RUNROOT/'consolidated38-v7/original-build'/config if role=='original' else PRIVATE/stage/config/'lib'
  dest=HERE/'metadata-v5'/stage/(role+'-'+config+'.json');dest.parent.mkdir(parents=True,exist_ok=True)
  r=command(stage+'-metadata-v5-'+role+'-'+config,[str(exe),str(lib),str(dest),str(HERE/'native-metadata-owners-v5.txt')]);receipts.append(clean(r));assert r['exit_code']==0
  files.append(pin(dest))
save('metadata-v5-'+stage+'.json',dict(commands=receipts,files=files,tool=pin(HERE/'MethodMetadata-v5.cs'),metadata_only=True,original_semantic_calls=0))
