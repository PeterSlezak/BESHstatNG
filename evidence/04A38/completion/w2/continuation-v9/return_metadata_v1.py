"""Bind exact current CLR return types to the full source/parameter contracts."""
from common_v2 import *
tool=PRIVATE/'return-metadata-v1';tool.mkdir(exist_ok=False);exe=tool/'ReturnMetadata.exe'
owners=set((HERE/'native-metadata-owners-v3.txt').read_text().splitlines());owners.update(['BESHStatNG.ExcelCommon.UdfAgreementReportPreparation38','BESHStatNG.ExcelCommon.UdfClassificationReportPreparation38','BESHStatNG.ExcelCommon.UdfContingencyPreparation38','BESHStatNG.Agreement.WeightedDemingRegression','BESHStatNG.ConfidenceIntervalResult'])
with (HERE/'return-metadata-owners-v1.txt').open('x',encoding='utf-8',newline='\n') as f:f.write('\n'.join(sorted(owners))+'\n')
r=command('return-metadata-tool-v1',[compiler('csc.exe'),'/nologo','/target:exe','/reference:System.Core.dll','/reference:System.Web.Extensions.dll','/out:'+str(exe),str(HERE/'ReturnMetadata-v1.cs')]);assert r['exit_code']==0
rows=[]
for config in ['Debug','Release']:
    output=HERE/('return-metadata-'+config+'-v1.json');r=command('return-metadata-'+config+'-v1',[str(exe),str(PRIVATE/'packet-v1'/config/'lib'),str(output),str(HERE/'return-metadata-owners-v1.txt')]);assert r['exit_code']==0;rows.append(pin(output))
save('return-metadata-authority-v1.json',dict(source=pin(HERE/'ReturnMetadata-v1.cs'),artifact=pin(exe),owners=pin(HERE/'return-metadata-owners-v1.txt'),results=rows,original_queries=0,original_activity='NONE; existing original source/CIL authority remains immutable'))
