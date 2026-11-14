"""Complete passive native/Common/engine context, no semantic original calls."""
from common import *
stage=sys.argv[1]
folder=PRIVATE/stage/'metadata-full';folder.mkdir(exist_ok=False)
source=(PREVIOUS/'MethodMetadata-v7.cs').read_text(encoding='utf-8')
needle='t.FullName.StartsWith("BESHStatNG.parametric.Parametric+OneWayANOVA",StringComparison.Ordinal)'
assert source.count(needle)==1
source=source.replace(needle,'t.FullName.StartsWith("BESHStatNG.parametric.",StringComparison.Ordinal)||RootType(t).Namespace=="BESHStatNG.nonparametric"||RootType(t).Namespace=="BESHStatNG.equivalencetests"||RootType(t).Namespace=="BESHStatNG.Matrix"')
source=source.replace('RootType(t).Name.Contains("38")','true')
owners=HERE/'native-metadata-owners-full.txt'
if not owners.exists():
 with owners.open('x',encoding='utf-8',newline='\n') as f:f.write((PREVIOUS/'native-metadata-owners-v7.txt').read_text(encoding='utf-8')+'\nBESHStatNG.ExcelDnaSharedApplicationAdapter\n')
src=HERE/'MethodMetadata-v9.cs'
if src.exists():assert src.read_text(encoding='utf-8')==source
else:
 with src.open('x',encoding='utf-8',newline='\n') as f:f.write(source)
exe=folder/'MethodMetadata38.exe'
r=command(stage+'-full-metadata-tool',[compiler('csc.exe'),'/nologo','/target:exe','/reference:System.Core.dll','/reference:System.Web.Extensions.dll','/out:'+str(exe),str(src)]);assert r['exit_code']==0
receipts=[clean(r)];entries=[]
for config in ['Debug','Release']:
 for origin,lib in [('current',PRIVATE/stage/config/'lib'),('original',RUNROOT/'consolidated38-v7/original-build'/config)]:
  out=HERE/'metadata-full'/stage/(origin+'-'+config+'.json');out.parent.mkdir(parents=True,exist_ok=True)
  r=command(stage+'-full-metadata-'+origin+'-'+config,[str(exe),str(lib),str(out),str(owners)]);assert r['exit_code']==0
  receipts.append(clean(r));entries.append(pin(out))
save('metadata-full-'+stage+'.json',dict(commands=receipts,files=entries,original_queries=0,original_activity='PASSIVE_METADATA_ONLY_NO_METHOD_OR_TYPE_INITIALIZER_EXECUTED'))
print('Full native/Common/parametric/rank/equivalence/matrix contexts captured',stage)
