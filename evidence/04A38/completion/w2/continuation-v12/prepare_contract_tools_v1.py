from common import *
rows=[]
for name in ['source_views.py','build_contracts_v3.py']:
    source=PREVIOUS/name;value=source.read_text(encoding='utf-8')
    if name=='build_contracts_v3.py':
        value=value.replace('from proof_kernel_v3 import','from proof_kernel_v4 import')
        value=value.replace('packet-v2-context-v5','packet-v1-context-v5').replace('current-universe-packet-v2.json','current-universe-packet-v1.json').replace('cohort-diagnostic-v2.json','cohort-diagnostic-v1.json').replace('declaration-shapes-v2.json','declaration-shapes-v1.json').replace("'-v2.json'","'-v1.json'")
        value=value.replace("if n['owner']!='BESHStatNG.ExcelCommon.UdfMultivariateReportPreparation38' and not any",'if not any')
        value=value.replace("if n['owner']!='BESHStatNG.ExcelCommon.UdfMultivariateReportPreparation38' and method['method'] not in reached", "if method['method'] not in reached")
        value=value.replace('Retained complete v10 authority','Retained complete v11 authority').replace('../continuation-v10/','../continuation-v11/')
    target=HERE/name
    with target.open('x',encoding='utf-8',newline='\n') as stream:stream.write(value)
    rows.append(dict(before=pin(source),after=pin(target),scope='C12 source/parameter/return/metadata/graph pointers and reached-only portable contract scope; exact existing binding algorithm retained.'))
save('contract-tools-source-crosswalk-v1.json',dict(rows=rows,original_queries=0))
