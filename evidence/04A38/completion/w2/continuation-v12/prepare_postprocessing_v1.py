from common import *
rows=[]
for name in ['report_validation_v1.py','postprocess_v1.py']:
    source=PREVIOUS/name;value=source.read_text(encoding='utf-8')
    value=value.replace('w2c11','w2c12').replace('w2c10','w2c11').replace('INHERITED_C10','INHERITED_C11').replace('NEW_C11','NEW_C12').replace('AUTHORIZED_C11','AUTHORIZED_C12').replace('C10_actual_parent_baseline','C11_actual_parent_baseline')
    if name=='postprocess_v1.py':
        value=value.replace("v10=pin(PREVIOUS/'python-first-failures-v2.json'),", "v11=pin(PREVIOUS/'python-first-failures-v2.json'),v10=pin(W2/'continuation-v10/python-first-failures-v2.json'),")
        a=value.index('    direct_observed_tool_attempts=[');b=value.index('    source_attempts=',a)
        value=value[:a]+"    direct_observed_tool_attempts=[],\n"+value[b:]
        value=value.replace('new_managed_PASS=29','new_managed_PASS=31')
    with (HERE/name).open('x',encoding='utf-8',newline='\n') as stream:stream.write(value)
    rows.append(dict(before=pin(source),after=pin(HERE/name),scope='Exact current labels, C11 baseline links and31 measured new managed cases. Test/scanner assertions untouched; new/lost/current source-pin deltas require actual investigation.'))
save('postprocessing-source-crosswalk-v1.json',dict(rows=rows,original_queries=0))
