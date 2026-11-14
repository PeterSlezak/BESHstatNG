from common import *
source=(HERE/'build_contracts_v2.py').read_text(encoding='utf-8')
point="    if n['parent_member'] is not None:continue\n    method=bind(n,metas['Debug'],shapes)"
replacement="    if n['parent_member'] is not None:continue\n    if n['owner']!='BESHStatNG.ExcelCommon.UdfMultivariateReportPreparation38' and not any((defs['Debug'][k]['declaring_type'],defs['Debug'][k]['member_name'])==(n['owner'],n['name']) for k in reached):continue\n    method=bind(n,metas['Debug'],shapes)"
assert point in source;source=source.replace(point,replacement)
with (HERE/'build_contracts_v3.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(source)
save('contract-builder-scope-correction-v3.json',dict(before=pin(HERE/'build_contracts_v2.py'),after=pin(HERE/'build_contracts_v3.py'),
    failure=pin(HERE/'contract-builder-execution-v2.json'),stderr=pin(RUNROOT/'commands/w2c11-contracts-v2/stderr.log'),
    cause='Portable census includes unrelated PlotData classes in the same physical source. The builder tried to bind them before limiting the set to actual graph owners.',
    correction='Select actually reached owner/name pairs before full parameter/rank/ByRef binding. All901 reached constructed methods and every C11 portable API remain required; no assertion or test changes.',original_queries=0))
result=command('contracts-v3',[sys.executable,'-B',str(HERE/'build_contracts_v3.py')])
save('contract-builder-execution-v3.json',clean(result));assert result['exit_code']==0
