from common import *
folder=PRIVATE/'runtime-metadata';folder.mkdir();exe=folder/'RuntimeMetadata.exe';vb=Path('C:/Program Files (x86)/Reference Assemblies/Microsoft/Framework/.NETFramework/v4.8/Microsoft.VisualBasic.dll')
r=command('runtime-metadata-compile',[compiler('csc.exe'),'/nologo','/reference:System.Core.dll','/reference:System.Web.Extensions.dll','/reference:'+str(vb),'/out:'+str(exe),str(HERE/'RuntimeMetadata.cs')]);assert r['exit_code']==0;records=[clean(r)]
out=HERE/'framework-runtime-metadata.json';tool=PRIVATE/'parametric-v4/metadata-v6/MethodMetadata38.exe'
r=command('runtime-metadata-read',[str(exe),str(tool),str(out)]);records.append(clean(r));assert r['exit_code']==0
save('runtime-metadata-commands.json',dict(commands=records,result=pin(out),scope='Current Framework metadata only; no statistical/helper method invoked'))
