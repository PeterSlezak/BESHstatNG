from common import *
def emit(name,text):
    with (HERE/name).open('x',encoding='utf-8',newline='\n') as stream:stream.write(text)
text=(HERE/'inspect_final_v3.py').read_text(encoding='utf-8').replace('cells-v4','cells-v5').replace('final-v3','final-v4').replace('final-inspection-v3','final-inspection-v4').replace('source-transitions-v2.json','source-transitions-v4.json').replace('source-after/final-v2','source-after/final-v4').replace("HERE / 'entry.json'","HERE / 'entry-v2.json'")
emit('inspect_final_v4.py',text)
text=(HERE/'run_dependency_final.py').read_text(encoding='utf-8').replace('cells-v4','cells-v5').replace('metadata-v8','metadata-v9')
emit('run_dependency_final_v2.py',text)
text=(HERE/'run_cell_probe_v2.py').read_text(encoding='utf-8').replace('cell-probe-v2-','cell-probe-v3-').replace('cell-probe-comparison-v2.json','cell-probe-comparison-v3.json')
emit('run_cell_probe_v3.py',text)
text=(HERE/'run_full_validation_v3.py').read_text(encoding='utf-8').replace('w2c5-validation-v3','w2c5-validation-v4').replace('full-managed-Debug-v3.json','full-managed-Debug-v4.json').replace('common-python-validation-v3.json','common-python-validation-v4.json').replace('serial-validation-v2.json','serial-validation-v4.json')
emit('run_full_validation_v4.py',text)
text=(HERE/'report_managed_final_v2.py').read_text(encoding='utf-8').replace('w2c5-validation-v3','w2c5-validation-v4').replace('focused-cells-v4-v2-','focused-cells-v5-').replace('selected-Core-cells-v4.json','selected-Core-cells-v5.json').replace('managed-Debug-identities-v2.json','managed-Debug-identities-v3.json').replace('managed-validation-v2.json','managed-validation-v3.json')
emit('report_managed_boundary.py',text)
