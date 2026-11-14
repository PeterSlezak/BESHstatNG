"""Fresh current CIL around the failing new copy-boundary assertion; original metadata reused only."""
from common import *
folder=PRIVATE/'bc-boxing-inspection-v1';folder.mkdir(exist_ok=False);exe=folder/'MethodMetadata38.exe'
r=command('boxing-metadata-tool-v1',[compiler('csc.exe'),'/nologo','/target:exe','/reference:System.Core.dll','/reference:System.Web.Extensions.dll','/out:'+str(exe),str(HERE/'MethodMetadata-v11.cs')]);assert r['exit_code']==0
output=HERE/'metadata-attempts/bc-boxing-v1/current-Debug.json';output.parent.mkdir(parents=True,exist_ok=True)
r=command('boxing-metadata-current-v1',[str(exe),str(PRIVATE/'packet-v2/Debug/lib'),str(output),str(HERE/'native-metadata-owners-v3.txt')]);assert r['exit_code']==0
rows=[]
for label,path in [('C9',PREVIOUS/'metadata-all/packet-v1-context-v5/current-Debug.json'),('C10',output)]:
    metadata=json.loads(path.read_bytes())
    for owner,name in [('BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs','FA_EIGEN'),('BESHStatNG.ExcelCommon.UdfMultivariateReportPreparation38','BuildFactorEigen')]:
        found=[r for r in metadata['methods'] if r['declaring_type']==owner and r['member_name']==name]
        for row in found:
            rows.append(dict(stage=label,method=row,metadata=pin(path)))
            print(label,owner,name)
            for i,ins in enumerate(row['instructions']):
                if ins['operand'] is not None and any(n in str(ins['operand']) for n in ['GetObjectValue','BuildFactorEigen','GetOptionalBool','PrepareResultTableForUdf']):print(i,row['instructions'][max(0,i-3):i+3])
save('boxing-copy-investigation-v1.json',dict(rows=rows,original_queries=0,scope='Passive metadata of real current consumers. Compares preserved C9 source/CIL with C10, not a source-text inference.'))
